import { dev } from '$app/environment';
import type { Feature, FeatureCollection, GeoJsonProperties, Geometry } from 'geojson';
import { LngLat } from 'maplibre-gl';
import { writable } from 'svelte/store';

const apiUrl = dev ? 'http://localhost:7071/api/' : 'https://geo-api.erikmagnusson.com/api/';
const tileZoom = 11;
const maxDistance = 100000;

// By tile index "x,y"
const queriedTiles = new Set<string>();
const ongoingFetches = new Map<string, { promise: Promise<void>; controller: AbortController }>();

function createPeaksStore() {
	const { subscribe, update, set } = writable({
		peaks: {
			type: 'FeatureCollection',
			features: [] as Feature<Geometry, GeoJsonProperties>[]
		} as FeatureCollection,
		summitedPeaks: {
			type: 'FeatureCollection',
			features: [] as Feature<Geometry, GeoJsonProperties>[]
		} as FeatureCollection
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
		var tileIndices = wsg84ToTileIndices(center, tileZoom);

		// Cancel ongoing fetches that are too far away
		for (const [key, { controller }] of ongoingFetches.entries()) {
			const [x, y] = key.split(',').map(Number);
			const tileLocation = tileIndicesToWsg84({ x, y }, tileZoom);
			if (calculateDistance(center, tileLocation) > maxDistance) {
				controller.abort();
			}
		}

		var fetchTasks = [];

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

	return {
		subscribe,
		fetchPeaks,
		reset: () => {
			set({
				peaks: {
					type: 'FeatureCollection',
					features: []
				},
				summitedPeaks: {
					type: 'FeatureCollection',
					features: []
				}
			});
		}
	};
}

export const peaksStore = createPeaksStore();

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