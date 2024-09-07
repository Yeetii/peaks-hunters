import type { FeatureCollection } from 'geojson';
import type { Map as MaplibreMap, StyleSetterOptions } from 'maplibre-gl';
import { get, writable } from 'svelte/store';
import { selectedPeaksGroups } from './filtersStore';
import { peaksStore } from './peaksStore';

export const MAPSTORE_CONTEXT_KEY = 'maplibre-map-store';

export type MapStore = ReturnType<typeof createMapStore>;

// map store for maplibre-gl object
export const createMapStore = () => {
	const { set, update, subscribe } = writable<MaplibreMap>(undefined);

	/**
	 * Update Maplibre's PaintProperty
	 *
	 * Note.
	 * setPaintProperty does render map canvas with new given property value.
	 * But in some cases, it does not actually update style.json object in Map instance.
	 * Because of this problem of Maplibre, the function is going to update style.json directly and call `setStyle` function.
	 *
	 * @param layerId The ID of the layer to set the paint property in.
	 * @param name The name of the paint property to set.
	 * @param value The value of the paint property to set. Must be of a type appropriate for the property, as defined in the MapLibre Style Specification.
	 * @param options Options object.
	 */
	const setPaintProperty = (
		layerId: string,
		name: string,
		value: unknown,
		options?: StyleSetterOptions
	) => {
		update((state) => {
			if (state) {
				state.setPaintProperty(layerId, name, value, options);

				const style = state.getStyle();
				const layer = style?.layers?.find((l) => l.id === layerId);
				if (layer) {
					if (!layer.paint) {
						layer.paint = {};
					}
					if (value) {
						// eslint-disable-next-line @typescript-eslint/ban-ts-comment
						// @ts-ignore
						layer.paint[name] = value;
					} else {
						// eslint-disable-next-line @typescript-eslint/ban-ts-comment
						// @ts-ignore
						if (layer.paint[name]) {
							// eslint-disable-next-line @typescript-eslint/ban-ts-comment
							// @ts-ignore
							delete layer.paint[name];
						}
					}
					state.setStyle(style);
				}
			}

			return state;
		});
	};

	/**
	 * Update Maplibre's LayoutProperty
	 *
	 * Note.
	 * setLayoutProperty does render map canvas with new given property value.
	 * But in some cases, it does not actually update style.json object in Map instance.
	 * Because of this problem of Maplibre, the function is going to update style.json directly and call `setStyle` function.
	 *
	 * @param layerId The ID of the layer to set the paint property in.
	 * @param name The name of the paint property to set.
	 * @param value The value of the paint property to set. Must be of a type appropriate for the property, as defined in the MapLibre Style Specification.
	 * @param options Options object.
	 */
	const setLayoutProperty = (
		layerId: string,
		name: string,
		value: unknown,
		options?: StyleSetterOptions
	) => {
		update((state) => {
			if (state) {
				state.setLayoutProperty(layerId, name, value, options);

				const style = state.getStyle();
				const layer = style?.layers?.find((l) => l.id === layerId);
				if (layer) {
					if (!layer.layout) {
						layer.layout = {};
					}
					if (value) {
						// eslint-disable-next-line @typescript-eslint/ban-ts-comment
						// @ts-ignore
						layer.layout[name] = value;
					} else {
						// eslint-disable-next-line @typescript-eslint/ban-ts-comment
						// @ts-ignore
						if (layer.layout[name]) {
							// eslint-disable-next-line @typescript-eslint/ban-ts-comment
							// @ts-ignore
							delete layer.layout[name];
						}
					}
					state.setStyle(style);
				}
			}

			return state;
		});
	};

	const updateMapSources = (peaks: FeatureCollection, summitedPeaks: FeatureCollection) => {
		update((map) => {
			if (map) {
				const peaksSource = map.getSource('peaks') as maplibregl.GeoJSONSource;
				const summitedPeaksSource = map.getSource('summitedPeaks') as maplibregl.GeoJSONSource;

				if (peaksSource) {
					peaksSource.setData(peaks);
				}
				if (summitedPeaksSource) {
					summitedPeaksSource.setData(summitedPeaks);
				}
				map.triggerRepaint();
			}
			return map;
		});
	};

	const filterPeaks = (
		peaks: FeatureCollection,
		summitedPeaks: FeatureCollection
	): { filteredPeaks: FeatureCollection; filteredSummitedPeaks: FeatureCollection } => {
		const selectedGroups = get(selectedPeaksGroups);

		if (selectedGroups.length === 0) {
			return { filteredPeaks: peaks, filteredSummitedPeaks: summitedPeaks };
		}

		const selectedPeakIds = selectedGroups.flatMap((group) => group.peakIds);

		const filteredPeaks: FeatureCollection = {
			type: 'FeatureCollection',
			features: peaks.features.filter(
				(feature) => feature.id !== undefined && selectedPeakIds.includes(feature.id as string)
			)
		};

		const filteredSummitedPeaks: FeatureCollection = {
			type: 'FeatureCollection',
			features: summitedPeaks.features.filter(
				(feature) => feature.id !== undefined && selectedPeakIds.includes(feature.id as string)
			)
		};

		return { filteredPeaks, filteredSummitedPeaks };
	};

	peaksStore.subscribe(({ peaks, summitedPeaks }) => {
		const { filteredPeaks, filteredSummitedPeaks } = filterPeaks(peaks, summitedPeaks);
		updateMapSources(filteredPeaks, filteredSummitedPeaks);
	});

	selectedPeaksGroups.subscribe(() => {
		const { peaks, summitedPeaks } = get(peaksStore);
		const { filteredPeaks, filteredSummitedPeaks } = filterPeaks(peaks, summitedPeaks);
		updateMapSources(filteredPeaks, filteredSummitedPeaks);
	});

	return {
		subscribe,
		update,
		set,
		setPaintProperty,
		setLayoutProperty
	};
};
