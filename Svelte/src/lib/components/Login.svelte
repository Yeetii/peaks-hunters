<script lang="ts">
	import stravaButton from '$lib/assets/btn_strava_connectwith_orange.png';
	import { createDialog, melt } from '@melt-ui/svelte';

	export let trigger: boolean = false;

	const {
		elements: { trigger: dialogTrigger, overlay, content, title, description, close, portalled },
		states: { open }
	} = createDialog({
		role: 'alertdialog',
		forceVisible: true
	});

	// Watch the trigger prop and update the dialog's open state
	$: open.set(trigger);
</script>

<button
	use:melt={$dialogTrigger}
	class="rounded-md bg-white px-4 py-2
    font-medium leading-none text-magnum-700 shadow-lg hover:opacity-75 absolute z-10"
>
	Login
</button>

{#if $open}
	<div class="" use:melt={$portalled}>
		<div use:melt={$overlay} class="fixed inset-0 z-50 bg-black/50" />
		<div
			class="fixed left-1/2 top-1/2 z-50 max-h-[85vh] w-[90vw]
            max-w-[450px] -translate-x-1/2 -translate-y-1/2 rounded-md bg-white
            p-6 shadow-lg"
			use:melt={$content}
		>
			<h2 use:melt={$title} class="m-0 text-lg font-medium text-black">Login at Strava</h2>
			<p use:melt={$description} class="mb-5 mt-2 leading-normal text-zinc-600">
				To use PeaksHunters you need to authenticate with Strava. We fetch all your activities from
				strava in order to calculate which peaks you've summited.
			</p>

			<div class="mt-6 flex justify-center">
				<button use:melt={$close} class="inline-flex">
					<img src={stravaButton} alt="Connect with strava" />
				</button>
			</div>

			<button
				use:melt={$close}
				aria-label="Close"
				class="absolute right-[10px] top-[10px] inline-flex h-6 w-6
                appearance-none items-center justify-center rounded-full text-magnum-800
                hover:bg-magnum-100 focus:shadow-magnum-400"
			>
				X
			</button>
		</div>
	</div>
{/if}
