import type { FeatureCollection } from 'geojson';
import { writable } from 'svelte/store';
import { config } from '../../config';
import { activeSession } from './sessionStore';

function createActivitiesStore() {
	const { subscribe, update } = writable({
		activities: { type: 'FeatureCollection', features: [] } as FeatureCollection
	});

	const fetchActivities = async () => {
		if (!document.cookie || !document.cookie.includes('pathsBeta=true')) {
			console.log('No pathsBeta cookie, skipping activities fetch');
			return;
		}
		fetch(`${config.apiUrl}activities`, { credentials: 'include' })
			.then((r) => r.json())
			.then((activities: FeatureCollection) => {
				if (activities.features.length === 0) return;
				update((store) => {
					store.activities.features = activities.features;
					return store;
				});
			})
			.catch((error) => {
				if (error.name === 'AbortError') {
					console.log('Fetch aborted');
				} else {
					console.error('Fetch error:', error);
				}
			});
	};

	activeSession.subscribe((value) => {
		if (value) {
			fetchActivities();
		}
	});

	return {
		subscribe,
		fetchActivities
	};
}

export const activitiesStore = createActivitiesStore();
