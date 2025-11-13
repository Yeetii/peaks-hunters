import { browser } from '$app/environment';
import type { Feature, FeatureCollection } from 'geojson';
import { LngLat } from 'maplibre-gl';
import { writable } from 'svelte/store';
import { activeSession } from './sessionStore';
import { signalRStore } from './signalRStore';
import { config } from '../../config';
import {
	loadFeatureCollectionFromLocalStorage,
	saveFeatureCollectionToLocalStorage
} from './localStorageHelper';

const tileZoom = 11;
const maxConcurrentFetches = 10;
// By tile index "x,y"
const queriedTiles = new Set<string>();
const ongoingFetches = new Array<AbortController>();

function createPeaksStore() {
	const initialSummitedPeaks = loadFeatureCollectionFromLocalStorage('summitedPeaks');

	const { subscribe, update } = writable({
		peaks: { type: 'FeatureCollection', features: [] } as FeatureCollection,
		summitedPeaks: initialSummitedPeaks
	});

	const fetchPeaksForTile = async (x: number, y: number, controller: AbortController) => {
		const tileKey = `${x},${y}`;
		await fetch(`${config.apiUrl}peaks/${x}/${y}`, { signal: controller.signal })
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
	};

	const killOldFetches = () => {
		while (ongoingFetches.length > maxConcurrentFetches) {
			const controller = ongoingFetches.shift();
			controller?.abort();
		}
	};

	const fetchPeaks = async (center: LngLat) => {
		const tileIndices = wsg84ToTileIndices(center, tileZoom);

		killOldFetches();

		for (let x = tileIndices.x - 1; x <= tileIndices.x + 1; x++) {
			for (let y = tileIndices.y - 1; y <= tileIndices.y + 1; y++) {
				const tileKey = `${x},${y}`;
				if (queriedTiles.has(tileKey)) continue;

				const controller = new AbortController();
				fetchPeaksForTile(x, y, controller);
				ongoingFetches.push(controller);
				queriedTiles.add(tileKey);
			}
		}
	};

	const fetchSummitedPeaks = async () => {
		fetch(`${config.apiUrl}summitedPeaks`, {
			credentials: 'include'
		}).then(async (response) => {
			if (!response.ok) {
				throw new Error('Network response was not ok');
			}
			const summitedPeaks = await response.json();
			update((store) => {
				store.summitedPeaks = summitedPeaks;
				saveFeatureCollectionToLocalStorage('summitedPeaks', summitedPeaks);
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
			saveFeatureCollectionToLocalStorage('summitedPeaks', store.summitedPeaks);
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
		fetch(`${config.apiUrl}gridIndices?peakIds=${peakIds}`).then(async (response) => {
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
