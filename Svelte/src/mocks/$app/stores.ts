import { writable } from 'svelte/store';

export const page = writable({
	url: new URL('http://localhost:3100'),
	params: {},
	route: { id: null },
	status: 200,
	error: null,
	data: {},
	form: undefined
});

export const navigating = writable(null);
export const updated = writable(false);
