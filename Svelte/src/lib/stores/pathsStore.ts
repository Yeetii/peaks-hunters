import type { FeatureCollection } from 'geojson';
import { LngLat } from 'maplibre-gl';
import { writable } from 'svelte/store';
import { config } from '../../config';

const tileZoom = 11;
const maxConcurrentFetches = 10;

// By tile index "x,y"
const queriedTiles = new Set<string>();
const ongoingFetches = new Array<AbortController>();

function createPathsStore() {
	const initialPaths = { type: 'FeatureCollection', features: [] } as FeatureCollection; //loadPathsFromLocalStorage('paths');

	const { subscribe, update } = writable({
		paths: initialPaths
	});

	const fetchPathsForTile = async (x: number, y: number, controller: AbortController) => {
		const tileKey = `${x},${y}`;
		fetch(`${config.apiUrl}paths/${x}/${y}`, { signal: controller.signal })
			.then((r) => r.json())
			.then((newPaths: FeatureCollection) => {
				if (newPaths.features.length === 0) return;
				update((store) => {
					store.paths.features = [...store.paths.features, ...newPaths.features];
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

	const fetchPaths = async (center: LngLat) => {
		if (!document.cookie || !document.cookie.includes('pathsBeta=true')) {
			console.log('No pathsBeta cookie, skipping paths fetch');
			return;
		}
		killOldFetches();

		const tileIndices = wsg84ToTileIndices(center, tileZoom);
		const tileKey = `${tileIndices.x},${tileIndices.y}`;

		if (queriedTiles.has(tileKey)) return;

		const controller = new AbortController();
		fetchPathsForTile(tileIndices.x, tileIndices.y, controller);
		ongoingFetches.push(controller);
		queriedTiles.add(tileKey);
	};

	return {
		subscribe,
		fetchPaths,
		fetchPathsForTile
	};
}

export const pathsStore = createPathsStore();

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
