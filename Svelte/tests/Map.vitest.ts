import { describe, it, expect } from 'vitest';
import { mount as svelteMount } from 'svelte';
import Map from '../src/lib/components/Map.svelte';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function mount(Component: any) {
	const target = document.createElement('div');
	document.body.appendChild(target);
	const instance = svelteMount(Component, { target });
	return { target, instance };
}

describe('Map.svelte', () => {
	it('renders map container', () => {
		const { target } = mount(Map);
		const el = target.querySelector('[data-testid="map"]');
		expect(el).toBeTruthy();
	});
});

describe('Map.svelte controls', () => {
	it('adds navigation control buttons', async () => {
		const { target } = mount(Map);
		await new Promise((r) => setTimeout(r, 5));
		expect(target.querySelector('.maplibregl-ctrl-zoom-in')).toBeTruthy();
		expect(target.querySelector('.maplibregl-ctrl-zoom-out')).toBeTruthy();
		expect(target.querySelector('.maplibregl-ctrl-compass')).toBeTruthy();
	});

	it('adds geolocate control', async () => {
		const { target } = mount(Map);
		await new Promise((r) => setTimeout(r, 5));
		expect(target.querySelector('.maplibregl-ctrl-geolocate')).toBeTruthy();
	});

	it('adds scale control', async () => {
		const { target } = mount(Map);
		await new Promise((r) => setTimeout(r, 5));
		expect(target.querySelector('.maplibregl-ctrl-scale')).toBeTruthy();
	});

	it('adds attribution control', async () => {
		const { target } = mount(Map);
		await new Promise((r) => setTimeout(r, 5));
		expect(target.querySelector('.maplibregl-ctrl-attrib')).toBeTruthy();
		expect(target.querySelector('.maplibregl-compact')).toBeTruthy();
	});
});
