import type { Feature } from 'geojson';
import { writable } from 'svelte/store';
import { config } from '../../config';

export interface PeakGroup {
	id: string;
	parentId?: string;
	name: string;
	amountOfPeaks: number;
	peakIds: string[];
	boundrary: Feature;
}

export const peaksGroups = writable<PeakGroup[]>([]);
export const selectedPeaksGroups = writable<PeakGroup[]>([]);

const fetchPeaksGroups = async () => {
	try {
		const response = await fetch(`${config.apiUrl}peaksGroups`);
		if (!response.ok) {
			throw new Error('Failed to fetch peaks groups');
		}
		const data = await response.json();
		peaksGroups.set(data);
	} catch (error) {
		console.error('Error fetching peaks groups:', error);
	}
};

fetchPeaksGroups();
