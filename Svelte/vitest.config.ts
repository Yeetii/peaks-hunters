import { defineConfig } from 'vitest/config';
import { sveltekit } from '@sveltejs/kit/vite';

export default defineConfig(({ mode }) => {
	const isTest = process.env.VITEST === 'true' || mode === 'test';
	return {
		// SvelteKit plugin provides both virtual module resolution ($app/*, $env/*) and Svelte compilation
		plugins: [sveltekit()],
		resolve: {
			conditions: isTest ? ['browser'] : []
		},
		define: {
			// Force non-SSR mode for component unit tests so Svelte lifecycle (onMount) is available
			'import.meta.env.SSR': false
		},
		test: {
			environment: 'jsdom',
			globals: true,
			setupFiles: ['./src/setupTests.ts', './tests/maplibre.mock.ts'],
			include: ['tests/**/*.vitest.{js,ts}', 'tests/**/*.unit.{js,ts}'],
			// Reduce noisy stack traces and speed up failures.
			clearMocks: true
		}
	};
});
