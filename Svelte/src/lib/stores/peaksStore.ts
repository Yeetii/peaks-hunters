import { dev } from '$app/environment';
import type { Feature, FeatureCollection, GeoJsonProperties, Geometry } from 'geojson';
import type { LngLat } from 'maplibre-gl';
import { writable } from 'svelte/store';

const apiUrl = dev ? 'http://localhost:7071/api/' : 'https://geo-api.erikmagnusson.com/api/';
const tileZoom = 11;

// By tile index "x,y"
const queriedTiles = new Set<string>();

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

	const fetchPeaksForTile = async (x: number, y: number) => {
		await fetch(`${apiUrl}peaks/${x}/${y}`)
			.then((r) => r.json())
			.then((newPeaks: FeatureCollection) => {
				update((store) => {
					newPeaks.features.forEach((peak) => {
						store.peaks.features.push(peak);
					});
					return store;
				});
			});
	};

	const fetchPeaks = async (center: LngLat) => {
		var tileIndices = wsg84ToTileIndices(center, tileZoom);

		var fetchTasks = [];

		for (let x = tileIndices.x - 1; x <= tileIndices.x + 1; x++) {
			for (let y = tileIndices.y - 1; y <= tileIndices.y + 1; y++) {
				const alreadyFetched = queriedTiles.has(`${x},${y}`);
				if (alreadyFetched) continue;
				fetchTasks.push(fetchPeaksForTile(x, y));
				queriedTiles.add(`${x},${y}`);
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
