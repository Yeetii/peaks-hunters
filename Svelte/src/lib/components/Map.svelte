<script lang="ts">
	import { PUBLIC_MAPTILER_KEY } from '$env/static/public';
	import type { MapStore } from '$lib/stores';
	import { MAPSTORE_CONTEXT_KEY } from '$lib/stores';
	import peakIcon from '$lib/assets/peak.png';
	import maplibregl, {
		AttributionControl,
		GeolocateControl,
		LngLat,
		Map,
		NavigationControl,
		ScaleControl
	} from 'maplibre-gl';
	import { getContext, onMount } from 'svelte';

	let mapStore: MapStore = getContext(MAPSTORE_CONTEXT_KEY);

	let mapContainer: HTMLDivElement;

	let radius = 20000

	onMount(() => {
		const map = new Map({
			container: mapContainer,
			style: `https://api.maptiler.com/maps/topo-v2/style.json?key=${PUBLIC_MAPTILER_KEY}`,
			center: [13.0509, 63.41698],
			zoom: 12,
			hash: true,
			attributionControl: false
		});
		map.addControl(new NavigationControl({}), 'top-right');
		map.addControl(
			new GeolocateControl({
				positionOptions: { enableHighAccuracy: true },
				trackUserLocation: true
			}),
			'top-right'
		);
		map.addControl(new ScaleControl({ maxWidth: 80, unit: 'metric' }), 'bottom-left');
		map.addControl(new AttributionControl({ compact: true }), 'bottom-right');

		mapStore?.set(map);

		map.on('load', () => {
			var lngLat = map.getCenter()
			map.loadImage(peakIcon)
				.then(image => map.addImage('peakIcon', image.data))

            fetch(`http://localhost:7071/api/peaks?lat=${lngLat.lat}&lon=${lngLat.lng}&radius=${radius}`)
                .then(r => r.json())
                .then(body => {

					map.addSource('places', {
					'type': 'geojson',
					'data': body});
					
					map.addLayer({
					'id': 'places',
					'type': 'symbol',
					'source': 'places',
					'layout': {
						'icon-image': 'peakIcon',
						'text-field': ['get', 'name'],
						'text-font': [
							'Open Sans Semibold',
							'Arial Unicode MS Bold'
						],
						'text-offset': [0, 1.25],
						'text-anchor': 'top'
						}
					});
                })

				map.on('click', 'places', (e) => {
					const description = e.features?.at(0)?.properties['elevation']

					const coordinates = e.lngLat

					// Ensure that if the map is zoomed out such that multiple
					// copies of the feature are visible, the popup appears
					// over the copy being pointed to.
					while (Math.abs(e.lngLat.lng - coordinates.lng) > 180) {
						coordinates.lng += e.lngLat.lng > coordinates.lng ? 360 : -360;
					}

					new maplibregl.Popup()
							.setLngLat(new LngLat(coordinates.lng, coordinates.lat))
							.setHTML(description)
							.addTo(map);
				});

				map.on('mouseenter', 'places', () => {
					map.getCanvas().style.cursor = 'pointer';
				});

				map.on('mouseleave', 'places', () => {
					map.getCanvas().style.cursor = '';
				});

				map.on('moveend', () => {
					var center = map.getCenter()
					// radius = map.getZoom()
					fetch(`http://localhost:7071/api/peaks?lat=${center.lat}&lon=${center.lng}&radius=${radius}`)
						.then(r => r.json())
						.then(body => {
							const placesSource = map.getSource('places') as maplibregl.GeoJSONSource
							placesSource.setData(body)
						})
				})
			}
		);
	});
</script>

<div class="map" data-testid="map" bind:this={mapContainer} />

<style>
	@import 'maplibre-gl/dist/maplibre-gl.css';

	.map {
		position: absolute;
		top: 0;
		bottom: 0;
		width: 100%;
		z-index: 1;
	}
</style>
