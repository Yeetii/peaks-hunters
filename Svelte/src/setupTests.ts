import { afterEach, vi } from 'vitest';

// Mock public env vars required by Map.svelte style URL.
vi.mock('$env/static/public', () => ({ PUBLIC_MAPTILER_KEY: 'test-key' }));

// Mock dynamic public env vars
vi.mock('$env/dynamic/public', () => ({
	env: {
		PUBLIC_MAPTILER_KEY: 'test-key',
		PUBLIC_API_URL: 'https://geo-api.erikmagnusson.com/api/'
	}
}));

// Mock fetch to prevent network calls in unit tests
global.fetch = vi.fn(() =>
	Promise.resolve({
		ok: true,
		json: () => Promise.resolve({ type: 'FeatureCollection', features: [] })
	})
) as any;

afterEach(() => {
	document.body.innerHTML = '';
	vi.clearAllMocks();
});
