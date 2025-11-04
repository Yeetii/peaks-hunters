import { writable } from 'svelte/store';

export type ComboboxOptionProps<T> = { value: T; label: string; disabled: boolean };

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function createCombobox<T>(_opts: any) {
	const open = writable(false);
	const inputValue = writable('');
	const touchedInput = writable(false);
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	const selected = writable<any>(undefined);
	return {
		// eslint-disable-next-line @typescript-eslint/no-explicit-any
		elements: { menu: {}, input: {}, option: (_o: any) => ({}), label: {} },
		states: { open, inputValue, touchedInput, selected },
		helpers: { isSelected: (_v: T) => false }
	};
}

// No-op action used via use:melt
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function melt(_node: HTMLElement, _params?: any) {
	return { update() {}, destroy() {} };
}
