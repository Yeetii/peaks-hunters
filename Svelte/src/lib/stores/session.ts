import { writable } from 'svelte/store';

// export type

export const activeSession = writable<boolean>(false);

// TODO: If there is a a cookie, make a call to backend to see if still valid
