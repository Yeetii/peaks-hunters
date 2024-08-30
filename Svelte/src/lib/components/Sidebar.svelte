<script lang="ts">
	import { dev } from '$app/environment';
	import { activeSession } from '$lib/stores';
	import { createEventDispatcher, onMount } from 'svelte';

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
		if ($activeSession) {
			return fetch(`${apiUrl}summitsStats`, {
				credentials: 'include'
			})
				.then((r) => {
					if (r.ok) {
						return r.json();
					}
					if (r.status === 401) {
						$activeSession = false;
					}
				})
				.then((stats: SummitsStats) => {
					summitsStats = stats;
				});
		}
	});
</script>

<div
	role="toolbar"
	class={`absolute flex justify-center items-center w-[300px] h-full transition-transform duration-1000 z-10 ${side} ${collapsed ? 'translate-x-[-295px]' : ''} ${side === 'right' && collapsed ? '!translate-x-[295px]' : ''}`}
>
	<div
		class="absolute w-[95%] h-[95%] bg-white rounded-[10px] shadow-[0_0_50px_-25px_black] font-mono text-sm text-gray-500 p-4"
	>
		<h2>Summits stats</h2>
		{#if summitsStats}
			<div class="flex flex-col gap-4 mt-6">
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
	<div
		role="button"
		tabindex="0"
		class={`absolute w-[1.3em] h-[1.3em] text-3xl font-sans text-gray-500 overflow-visible flex justify-center items-center bg-white rounded-[10px] shadow-[0_0_50px_-25px_black] ${side === 'left' ? 'right-[-1.5em]' : 'left-[-1.5em]'} hover:text-[#0aa1cf] hover:cursor-pointer`}
		on:click={toggleSidebar}
		on:keydown={(e) => e.key === 'Enter' && toggleSidebar()}
	>
		{#if side === 'left'}
			→
		{:else}
			←
		{/if}
	</div>
</div>
