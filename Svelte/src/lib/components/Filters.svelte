<script lang="ts">
	import { createCombobox, melt, type ComboboxOptionProps } from '@melt-ui/svelte';
	import { fly } from 'svelte/transition';
	import { peaksGroups, selectedPeaksGroups, type PeakGroup } from '../stores/filtersStore';

	const toOption = (peakGroup: PeakGroup): ComboboxOptionProps<PeakGroup> => ({
		value: peakGroup,
		label: peakGroup.name,
		disabled: false
	});

	const {
		elements: { menu, input, option, label },
		states: { open, inputValue, touchedInput, selected },
		helpers: { isSelected }
	} = createCombobox<PeakGroup>({
		forceVisible: true,
		// @ts-expect-error - Melt component does not have correct types
		multiple: true
	});

	selected.subscribe((selected) => {
		selectedPeaksGroups.update((store) => {
			// @ts-expect-error - Can't bother typing the melt component
			store = Array.isArray(selected) ? selected.map(({ value }) => value) : [];
			return store;
		});
	});

	$: if (!$open) {
		// @ts-expect-error - Melt component does not have correct types?
		$inputValue = $selected?.label ?? '';
	}

	$: filteredPeakGroups = $touchedInput
		? $peaksGroups.filter(({ name }) => {
				const normalizedInput = $inputValue.toLowerCase();
				return name.toLowerCase().includes(normalizedInput);
			})
		: $peaksGroups;
</script>

<div class="flex flex-col gap-1">
	<label use:melt={$label}>
		<span class="text-sm font-medium text-magnum-900">Filter peaks by collection: </span>
		<div class="relative">
			<input
				use:melt={$input}
				class="flex h-10 items-center justify-between rounded-lg bg-white
                px-3 pr-12 text-black"
				placeholder="Jämtlands fjäll..."
			/>
			<div class="absolute right-2 top-1/2 z-10 -translate-y-1/2 text-magnum-900">
				{#if $open}
					⬆︎
				{:else}
					⬇︎
				{/if}
			</div>
		</div>
	</label>
</div>
{#if $open}
	<ul
		class=" z-10 flex max-h-[300px] flex-col overflow-hidden rounded-lg"
		use:melt={$menu}
		transition:fly={{ duration: 150, y: -5 }}
	>
		<div
			class="flex max-h-full flex-col gap-0 overflow-y-auto bg-white px-2 py-2 text-black"
			tabindex="0"
			role="listbox"
		>
			{#each filteredPeakGroups as peaksGroup, index (index)}
				<li
					use:melt={$option(toOption(peaksGroup))}
					class="relative cursor-pointer scroll-my-2 rounded-md py-2 pl-4 pr-4
          hover:bg-magnum-100
          data-[highlighted]:bg-magnum-200 data-[highlighted]:text-magnum-900
            data-[disabled]:opacity-50"
				>
					{#if $isSelected(peaksGroup)}
						<div class="check absolute left-2 top-1/2 z-10 text-magnum-900">✔︎</div>
					{/if}
					<div class="pl-4">
						<span class="font-medium">{peaksGroup.name}</span>
						<span class="block text-sm opacity-75">{peaksGroup.amountOfPeaks}</span>
					</div>
				</li>
			{:else}
				<li class="relative cursor-pointer rounded-md py-1 pl-8 pr-4">No results found</li>
			{/each}
		</div>
	</ul>
{/if}

<style lang="postcss">
	.check {
		@apply absolute left-2 top-1/2 text-magnum-500;
		translate: 0 calc(-50% + 1px);
	}
</style>
