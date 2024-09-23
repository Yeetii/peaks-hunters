import { browser, dev } from '$app/environment';
import type { Feature, FeatureCollection } from 'geojson';
import { LngLat } from 'maplibre-gl';
import { writable } from 'svelte/store';
import { activeSession } from './sessionStore';
import { signalRStore } from './signalRStore';

const apiUrl = dev ? 'http://localhost:7071/api/' : 'https://geo-api.erikmagnusson.com/api/';
const tileZoom = 11;
const maxDistance = 100000;

// By tile index "x,y"
const queriedTiles = new Set<string>();
const ongoingFetches = new Map<string, { promise: Promise<void>; controller: AbortController }>();

function createPeaksStore() {
	const emptyFeatureCollection: FeatureCollection = { type: 'FeatureCollection', features: [] };
	const initialSummitedPeaks = loadPeaksFromLocalStorage('summitedPeaks');
	const initialPeaks = emptyFeatureCollection;

	const { subscribe, update } = writable({
		peaks: initialPeaks,
		summitedPeaks: initialSummitedPeaks
	});

	const fetchPeaksForTile = async (x: number, y: number, controller: AbortController) => {
		const tileKey = `${x},${y}`;
		await fetch(`${apiUrl}peaks/${x}/${y}`, { signal: controller.signal })
			.then((r) => r.json())
			.then((newPeaks: FeatureCollection) => {
				update((store) => {
					newPeaks.features.forEach((peak) => {
						store.peaks.features.push(peak);
					});
					return store;
				});
			})
			.catch((error) => {
				queriedTiles.delete(tileKey);
				if (error.name === 'AbortError') {
					console.log('Fetch aborted');
				} else {
					console.error('Fetch error:', error);
				}
			});
		ongoingFetches.delete(tileKey);
	};

	const fetchPeaks = async (center: LngLat) => {
		const tileIndices = wsg84ToTileIndices(center, tileZoom);

		// TODO: Check if simply removing the older fetches would be sufficient, only allowing ~15  fetches at a time
		// Cancel ongoing fetches that are too far away
		for (const [key, { controller }] of ongoingFetches.entries()) {
			const [x, y] = key.split(',').map(Number);
			const tileLocation = tileIndicesToWsg84({ x, y }, tileZoom);
			if (calculateDistance(center, tileLocation) > maxDistance) {
				controller.abort();
			}
		}

		const fetchTasks = [];

		for (let x = tileIndices.x - 1; x <= tileIndices.x + 1; x++) {
			for (let y = tileIndices.y - 1; y <= tileIndices.y + 1; y++) {
				const tileKey = `${x},${y}`;
				if (queriedTiles.has(tileKey)) continue;

				const controller = new AbortController();
				const fetchPromise = fetchPeaksForTile(x, y, controller);
				ongoingFetches.set(tileKey, { promise: fetchPromise, controller });
				fetchTasks.push(fetchPromise);
				queriedTiles.add(tileKey);
			}
		}

		await Promise.all(fetchTasks);
	};

	const fetchSummitedPeaks = async () => {
		fetch(`${apiUrl}summitedPeaks`, {
			credentials: 'include'
		}).then(async (response) => {
			if (!response.ok) {
				throw new Error('Network response was not ok');
			}
			const summitedPeaks = await response.json();
			update((store) => {
				store.summitedPeaks = summitedPeaks;
				saveSummitedPeaksToLocalStorage(summitedPeaks);
				return store;
			});
		});
	};

	const addSummitedPeak = (newSummitedPeak: Feature) => {
		update((store) => {
			if (store.summitedPeaks.features.find((peak) => peak.id === newSummitedPeak.id)) {
				return store;
			}

			store.summitedPeaks.features.push(newSummitedPeak);
			saveSummitedPeaksToLocalStorage(store.summitedPeaks);
			return store;
		});
	};

	activeSession.subscribe((activeSession) => {
		if (activeSession) {
			fetchSummitedPeaks();
		}
	});

	signalRStore.subscribe((message) => {
		console.log('SignalR message:', message);
	});

	const fetchPeaksByIds = async (peakIds: string[]) => {
		fetch(`${apiUrl}gridIndices?peakIds=${peakIds}`).then(async (response) => {
			if (!response.ok) {
				throw new Error('Network response was not ok');
			}
			const gridIndices: string[] = await response.json();
			gridIndices.forEach((tileKey) => {
				const indices = tileKey.split(',').map(Number);
				fetchPeaksForTile(indices[0], indices[1], new AbortController());
				queriedTiles.add(tileKey);
			});
		});
	};

	return {
		subscribe,
		fetchPeaks,
		fetchPeaksByIds,
		addSummitedPeak
	};
}

export const peaksStore = createPeaksStore();

function saveSummitedPeaksToLocalStorage(summitedPeaks: FeatureCollection) {
	localStorage.setItem('summitedPeaks', JSON.stringify(summitedPeaks));
}

function loadPeaksFromLocalStorage(collectionName: 'peaks' | 'summitedPeaks'): FeatureCollection {
	const emptyFeatureCollection: FeatureCollection = { type: 'FeatureCollection', features: [] };
	if (browser) {
		const storedPeaks = localStorage.getItem(collectionName);
		if (storedPeaks) {
			try {
				return JSON.parse(storedPeaks) as FeatureCollection;
			} catch {
				return emptyFeatureCollection;
			}
		}
	}
	return emptyFeatureCollection;
}

function calculateDistance(coord1: LngLat, coord2: LngLat): number {
	const R = 6371e3; // Earth's radius in meters
	const φ1 = (coord1.lat * Math.PI) / 180;
	const φ2 = (coord2.lat * Math.PI) / 180;
	const Δφ = ((coord2.lat - coord1.lat) * Math.PI) / 180;
	const Δλ = ((coord2.lng - coord1.lng) * Math.PI) / 180;

	const a =
		Math.sin(Δφ / 2) * Math.sin(Δφ / 2) +
		Math.cos(φ1) * Math.cos(φ2) * Math.sin(Δλ / 2) * Math.sin(Δλ / 2);
	const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));

	return R * c;
}

function tileIndicesToWsg84(tileIndices: { x: number; y: number }, zoom: number): LngLat {
	const n = Math.pow(2, zoom);
	const lon = (tileIndices.x / n) * 360 - 180;
	const lat = (Math.atan(Math.sinh(Math.PI * (1 - (2 * tileIndices.y) / n))) * 180) / Math.PI;
	return new LngLat(lon, lat);
}

function wsg84ToTileIndices(coord: LngLat, zoom: number): { x: number; y: number } {
	const lon = coord.lng;
	const lat = coord.lat;
	const n = Math.pow(2, zoom);
	const xtile = Math.floor(((lon + 180) / 360) * n);
	const ytile = Math.floor(
		((1 -
			Math.log(Math.tan((lat * Math.PI) / 180) + 1 / Math.cos((lat * Math.PI) / 180)) / Math.PI) /
			2) *
			n
	);
	return {
		x: xtile,
		y: ytile
	};
}
