import { vi } from 'vitest';

// Clean mock for maplibre-gl with control DOM elements for Vitest.
vi.mock('maplibre-gl', () => {
	class LngLat {
		constructor(
			public lng: number,
			public lat: number
		) {}
	}
	class NavigationControl {
		onAdd() {
			const root = document.createElement('div');
			root.className = 'maplibregl-ctrl maplibregl-ctrl-navigation';
			['zoom-in', 'zoom-out', 'compass'].forEach((name) => {
				const btn = document.createElement('button');
				btn.className = `maplibregl-ctrl-${name}`;
				root.appendChild(btn);
			});
			return root;
		}
		onRemove() {}
	}
	class GeolocateControl {
		onAdd() {
			const el = document.createElement('button');
			el.className = 'maplibregl-ctrl-geolocate';
			return el;
		}
		onRemove() {}
	}
	class ScaleControl {
		onAdd() {
			const el = document.createElement('div');
			el.className = 'maplibregl-ctrl-scale';
			return el;
		}
		onRemove() {}
	}
	class AttributionControl {
		onAdd() {
			const el = document.createElement('div');
			el.className = 'maplibregl-ctrl-attrib maplibregl-compact';
			el.textContent = '© Test Attribution';
			return el;
		}
		onRemove() {}
	}
	class TerrainControl {
		onAdd() {
			const el = document.createElement('div');
			el.className = 'maplibregl-ctrl-terrain';
			return el;
		}
		onRemove() {}
	}
	class MockMap {
		_container: HTMLElement;
		_style: { layers: any[] } = { layers: [] };
		_sources: Record<string, any> = {};
		_events: Record<string, Function[]> = {};
		_images: Record<string, any> = {};
		constructor(opts: any) {
			this._container = opts.container;
			setTimeout(() => this._emit('load'), 0);
		}
		_emit(evt: string, ...args: any[]) {
			(this._events[evt] || []).forEach((fn) => fn(...args));
		}
		on(event: string, cb: any) {
			if (!this._events[event]) this._events[event] = [];
			this._events[event].push(cb);
		}
		addControl(control: any) {
			if (control?.onAdd) {
				const el = control.onAdd(this);
				this._container.appendChild(el);
			}
		}
		addSource(name: string, source: any) {
			this._sources[name] = { ...source, setData: (d: any) => (this._sources[name].data = d) };
		}
		addLayer(layer: any) {
			this._style.layers.push(layer);
		}
		loadImage(_img: any) {
			return Promise.resolve({ data: '' });
		}
		addImage(name: string, data: any) {
			this._images[name] = data;
		}
		getCenter() {
			return { lng: 0, lat: 0, toString: () => '0,0' };
		}
		getZoom() {
			return 12;
		}
		getCanvas() {
			return { style: {} } as any;
		}
		easeTo() {}
		triggerRepaint() {}
		getStyle() {
			return this._style as any;
		}
		getSource(name: string) {
			return this._sources[name];
		}
	}
	return {
		default: { TerrainControl, Map: MockMap },
		Map: MockMap,
		LngLat,
		NavigationControl,
		GeolocateControl,
		ScaleControl,
		AttributionControl,
		TerrainControl
	};
});
