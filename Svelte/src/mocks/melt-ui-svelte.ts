import { writable } from 'svelte/store';

export type ComboboxOptionProps<T> = { value: T; label: string; disabled: boolean };

export function createCombobox<T>(_opts: any) {
	const open = writable(false);
	const inputValue = writable('');
	const touchedInput = writable(false);
	const selected = writable<any>(undefined);
	return {
		elements: { menu: {}, input: {}, option: (_o: any) => ({}), label: {} },
		states: { open, inputValue, touchedInput, selected },
		helpers: { isSelected: (_v: T) => false }
	};
}

// No-op action used via use:melt
export function melt(_node: HTMLElement, _params?: any) {
	return { update() {}, destroy() {} };
}
