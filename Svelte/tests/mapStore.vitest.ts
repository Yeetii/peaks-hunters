import { describe, it, expect } from 'vitest';
import { createMapStore } from '../src/lib/stores/mapStore';

describe('mapStore', () => {
	it('creates map store API', () => {
		const store = createMapStore();
		expect(store).toBeTruthy();
		// Basic shape assertions
		expect(typeof (store as any).subscribe).toBe('function');
		expect(typeof (store as any).setPaintProperty).toBe('function');
	});
});
