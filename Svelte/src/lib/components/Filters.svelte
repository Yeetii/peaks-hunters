<script lang="ts">
	import { peaksStore } from '$lib/stores/peaksStore';
	import { createCombobox, melt, type ComboboxOptionProps } from '@melt-ui/svelte';
	import type { FeatureCollection } from 'geojson';
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

	const summitedPeaksInGroup = (
		summitedPeaks: FeatureCollection,
		peaksInGroup: string[]
	): number => {
		const peaksInGroupIds = new Set(peaksInGroup);
		const summitedPeaksIds = new Set(summitedPeaks.features.map((p) => p.id));
		return peaksInGroupIds.intersection(summitedPeaksIds).size;
	};
</script>

<div class="flex flex-col gap-1">
	<label use:melt={$label}>
		<span class="text-sm font-medium text-gray-800">Filter peaks by collection: </span>
		<div class="relative">
			<input
				use:melt={$input}
				class="flex h-10 items-center justify-between rounded-lg bg-white
                px-3 pr-12 text-black"
				placeholder="Jämtlands fjäll..."
			/>
			<div class="absolute right-2 top-1/2 z-10 -translate-y-1/2">
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
		class=" z-10 flex max-h-[400px] flex-col overflow-hidden rounded-lg"
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
          hover:bg-gray-100
          data-[highlighted]:bg-gray-200 data-[highlighted]:text-gray-800
            data-[disabled]:opacity-50"
				>
					{#if $isSelected(peaksGroup)}
						<div class="check absolute left-2 top-1/2 z-10 text-gray-800">✔︎</div>
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
{:else}
	<h4 class="mt-4">Selected filters:</h4>
	<ul>
		{#each $selectedPeaksGroups as group}
			<li>
				{group.name}
				{summitedPeaksInGroup($peaksStore.summitedPeaks, group.peakIds)}/{group.amountOfPeaks}
			</li>
		{/each}
	</ul>
	{#if $selectedPeaksGroups.length > 0}
		<button
			on:click={() => selected.set(undefined)}
			class="mt-2 px-4 py-2 bg-gray-500 text-white rounded-md hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-gray-300"
		>
			Clear Filters
		</button>
	{/if}
{/if}

<style>
	.check {
		position: absolute;
		color: #af9e03;
		translate: 0 calc(-50% + 1px);
		font-size: 25px;
	}
</style>
