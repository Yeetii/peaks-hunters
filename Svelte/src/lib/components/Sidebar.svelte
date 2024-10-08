<script lang="ts">
	import { dev } from '$app/environment';
	import { activeSession } from '$lib/stores';
	import { createEventDispatcher, onMount } from 'svelte';
	import { fade } from 'svelte/transition';
	import Filters from './Filters.svelte';

	export let side: 'left' | 'right' = 'left';
	export let collapsed = true;
	export let content = '';

	const apiUrl = dev ? 'http://localhost:7071/api/' : 'https://geo-api.erikmagnusson.com/api/';

	const dispatch = createEventDispatcher();

	interface SummitsStats {
		totalPeaksClimbed: number;
		totalPeaksClimbedCategorized: number[];
		mostVisitedPeaks: VisitedPeak[];
	}

	interface VisitedPeak {
		id: string;
		name: string;
		count: number;
		elevation: number;
	}

	let summitsStats: SummitsStats;

	function toggleSidebar() {
		collapsed = !collapsed;
		dispatch('toggle', { side, collapsed });
	}

	onMount(() => {
		activeSession.subscribe((value) => {
			if (value) {
				fetch(`${apiUrl}summitsStats`, {
					credentials: 'include'
				})
					.then((r) => {
						if (r.ok) {
							return r.json();
						}
						if (r.status === 401) {
							activeSession.set(false);
						}
					})
					.then((stats: SummitsStats) => {
						summitsStats = stats;
					});
			}
		});
	});

	function handleKeydown(event: KeyboardEvent) {
		if (event.ctrlKey && event.key === 'b') {
			toggleSidebar();
		}
	}
	let showTooltip = false;
</script>

<svelte:window on:keydown={handleKeydown} />

<div
	role="toolbar"
	class={`fixed flex justify-center items-start w-[300px] h-full transition-transform duration-1000 z-10 ${side} ${collapsed ? 'translate-x-[-295px]' : ''} ${side === 'right' && collapsed ? '!translate-x-[295px]' : ''}`}
>
	<div
		class="w-[95%] h-[calc(100vh-2rem)] bg-white rounded-[10px] shadow-[0_0_50px_-25px_black] font-mono text-sm text-gray-500 p-4 overflow-y-auto"
	>
		<div>
			<h2 class="mb-6">Summits stats</h2>
			{#if summitsStats}
				<div class="flex flex-col gap-4">
					<p>Total summits: {summitsStats.totalPeaksClimbed}</p>
					<ol>
						{#each summitsStats.mostVisitedPeaks as peak, index}
							<li>{index + 1}. {peak.name} ({peak.elevation}m) ({peak.count}x)</li>
						{/each}
					</ol>
					<ul>
						{#each summitsStats.totalPeaksClimbedCategorized as count, index}
							<li>{index == 0 ? '<1000' : '≥' + index * 1000}m: {count}</li>
						{/each}
					</ul>
				</div>
			{:else}
				{content}
			{/if}
		</div>
		<div>
			<h2 class="mt-6">Filters</h2>
			<Filters />
		</div>
	</div>
	<div
		role="button"
		tabindex="0"
		on:mouseenter={() => (showTooltip = true)}
		on:mouseleave={() => (showTooltip = false)}
		class={`absolute top-1/2 transform -translate-y-1/2 w-[1.3em] h-[1.3em] text-3xl font-sans text-gray-500 overflow-visible flex justify-center items-center bg-white rounded-[10px] shadow-[0_0_50px_-25px_black] ${side === 'left' ? 'right-[-1.5em]' : 'left-[-1.5em]'} hover:text-[#0aa1cf] hover:cursor-pointer`}
		on:click={toggleSidebar}
		on:keydown={(e) => e.key === 'Enter' && toggleSidebar()}
	>
		{#if collapsed}
			→
		{:else}
			←
		{/if}

		{#if showTooltip}
			<div
				transition:fade={{ duration: 500 }}
				class="absolute {side === 'left'
					? 'left-[100%] ml-2'
					: 'right-[100%] mr-2'} whitespace-nowrap bg-gray-800 text-white text-sm rounded px-2 py-1"
			>
				{collapsed ? 'Open' : 'Close'} sidebar (Ctrl+B)
			</div>
		{/if}
	</div>
</div>
