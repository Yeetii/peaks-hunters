import { describe, it, expect } from 'vitest';
import { mount as svelteMount } from 'svelte';
import TopPage from '../src/routes/+page.svelte';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function mount(Component: any) {
	const target = document.createElement('div');
	document.body.appendChild(target);
	const instance = svelteMount(Component, { target });
	return { target, instance };
}

describe('+page.svelte', () => {
	it('shows map element', () => {
		const { target } = mount(TopPage);
		expect(target.querySelector('.map')).toBeTruthy();
	});
});
