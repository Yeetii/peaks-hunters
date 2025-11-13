import { browser } from '$app/environment';
import type { FeatureCollection } from 'geojson';

function saveFeatureCollectionToLocalStorage(
	collectionName: string,
	featureCollection: FeatureCollection
) {
	localStorage.setItem(collectionName, JSON.stringify(featureCollection));
}

function loadFeatureCollectionFromLocalStorage(collectionName: string): FeatureCollection {
	const emptyFeatureCollection: FeatureCollection = { type: 'FeatureCollection', features: [] };
	if (browser) {
		const storedFeatures = localStorage.getItem(collectionName);
		if (storedFeatures) {
			try {
				return JSON.parse(storedFeatures) as FeatureCollection;
			} catch {
				return emptyFeatureCollection;
			}
		}
	}
	return emptyFeatureCollection;
}

export { saveFeatureCollectionToLocalStorage, loadFeatureCollectionFromLocalStorage };
