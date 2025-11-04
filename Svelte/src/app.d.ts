// See https://kit.svelte.dev/docs/types#app
// for information about these interfaces
// and what to do when importing types
declare namespace App {
	// interface Error {}
	// interface Locals {}
	// interface PageData {}
	// interface Platform {}
}

declare module '$env/dynamic/public' {
	export const env: {
		PUBLIC_VITE_API_URL?: string;
		PUBLIC_MAPTILER_KEY: string;
		PUBLIC_API_URL?: string;
	};
}
