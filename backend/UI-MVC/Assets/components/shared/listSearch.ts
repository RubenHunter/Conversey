type SearchRootConfig = {
	root: HTMLElement;
	input: HTMLInputElement;
	items: HTMLElement[];
}

function normalize(value: string): string {
	return value.toLowerCase().trim();
}

function resolveItems(root: HTMLElement): HTMLElement[] {
	const selector = root.getAttribute("data-search-items") || "[data-search-item]";
	return Array.from(root.querySelectorAll<HTMLElement>(selector));
}

function resolveInput(root: HTMLElement): HTMLInputElement | null {
	const selector = root.getAttribute("data-search-input");
	if (!selector) {
		return root.querySelector<HTMLInputElement>("[data-search-input]");
	}
	return document.querySelector<HTMLInputElement>(selector);
}

function getItemText(item: HTMLElement): string {
	const explicit = item.getAttribute("data-search-text");
	if (explicit) {
		return explicit;
	}
	return item.textContent || "";
}

function applyFilter(config: SearchRootConfig, rawQuery: string): void {
	const query = normalize(rawQuery);
	let visibleCount = 0;

	config.items.forEach((item) => {
		if (item.hasAttribute("data-search-ignore")) {
			item.classList.remove("hidden");
			return;
		}

		const text = normalize(getItemText(item));
		const matches = query.length === 0 || text.includes(query);
		item.classList.toggle("hidden", !matches);
		if (matches) {
			visibleCount += 1;
		}
	});

	const counterSelector = config.root.getAttribute("data-search-count");
	if (counterSelector) {
		const counter = document.querySelector<HTMLElement>(counterSelector);
		if (counter) {
			counter.textContent = String(visibleCount);
		}
	}
}

function setupSearch(root: HTMLElement): void {
	const input = resolveInput(root);
	if (!input) {
		return;
	}

	const config: SearchRootConfig = {
		root,
		input,
		items: resolveItems(root)
	};

	let frame: number | null = null;
	const onInput = () => {
		if (frame) {
			cancelAnimationFrame(frame);
		}
		frame = requestAnimationFrame(() => {
			applyFilter(config, input.value);
		});
	};

	input.addEventListener("input", onInput);
	applyFilter(config, input.value);
}

function initListSearch(): void {
	const roots = Array.from(document.querySelectorAll<HTMLElement>("[data-search-root]"));
	roots.forEach(setupSearch);
}

document.addEventListener("DOMContentLoaded", initListSearch);
