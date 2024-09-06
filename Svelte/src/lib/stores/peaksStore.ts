import { dev } from '$app/environment';
import type { Feature, FeatureCollection, GeoJsonProperties, Geometry } from 'geojson';
import type { LngLat } from 'maplibre-gl';
import { writable } from 'svelte/store';

const apiUrl = dev ? 'http://localhost:7071/api/' : 'https://geo-api.erikmagnusson.com/api/';
const fetchRadius = 50000;
const cancelFetchZone = 0.5;

function createPeaksStore() {
	const { subscribe, update, set } = writable({
		peaks: {
			type: 'FeatureCollection',
			features: [] as Feature<Geometry, GeoJsonProperties>[]
		} as FeatureCollection,
		summitedPeaks: {
			type: 'FeatureCollection',
			features: [] as Feature<Geometry, GeoJsonProperties>[]
		} as FeatureCollection,
		queriedLocations: [] as LngLat[],
		peakIds: new Set<string>()
	});

	return {
		subscribe,
		fetchPeaks: async (center: LngLat) => {
			update((store) => {
				const alreadyFetched = store.queriedLocations.some((lngLat) => {
					const distance = calculateDistance(lngLat, center);
					return distance < fetchRadius * cancelFetchZone;
				});

				if (alreadyFetched) return store;

				store.queriedLocations.push(center);

				fetch(`${apiUrl}peaks?lat=${center.lat}&lon=${center.lng}&radius=${fetchRadius}`, {
					credentials: 'include'
				})
					.then((r) => r.json())
					.then((newPeaks: FeatureCollection) => {
						const filteredPeaks = newPeaks.features.filter(
							(feature) => !store.peakIds.has(feature.id as string)
						);
						filteredPeaks.forEach((peak) => {
							store.peakIds.add(peak.id as string);
							if (peak.properties && peak.properties['summited']) {
								store.summitedPeaks.features.push(peak);
							} else {
								store.peaks.features.push(peak);
							}
						});
					});

				return store;
			});
		},
		reset: () => {
			set({
				peaks: {
					type: 'FeatureCollection',
					features: []
				},
				summitedPeaks: {
					type: 'FeatureCollection',
					features: []
				},
				queriedLocations: [],
				peakIds: new Set()
			});
		}
	};
}

export const peaksStore = createPeaksStore();

function toRadians(degrees: number): number {
	return degrees * (Math.PI / 180);
}

const R = 6371e3; // Earth's radius in meters
function calculateDistance(p1: LngLat, p2: LngLat): number {
	const φ1 = toRadians(p1.lat);
	const φ2 = toRadians(p2.lat);
	const Δφ = toRadians(p2.lat - p1.lat);
	const Δλ = toRadians(p2.lng - p1.lng);

	const a =
		Math.sin(Δφ / 2) * Math.sin(Δφ / 2) +
		Math.cos(φ1) * Math.cos(φ2) * Math.sin(Δλ / 2) * Math.sin(Δλ / 2);
	const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));

	return R * c; // Distance in meters
}
