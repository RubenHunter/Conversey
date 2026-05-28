function initAdminHeaderHeight(header: HTMLElement): void {
	const syncAdminHeaderHeight = (): void => {
		document.documentElement.style.setProperty("--admin-header-height", `${header.offsetHeight}px`);
	};

	syncAdminHeaderHeight();
	window.addEventListener("resize", syncAdminHeaderHeight);
}

function initLanguageDropdowns(): void {
	const dropdowns = Array.from(document.querySelectorAll<HTMLElement>(".lang-dropdown"));
	dropdowns.forEach((dropdown) => {
		const toggle = dropdown.querySelector<HTMLButtonElement>(".lang-toggle");
		const menu = dropdown.querySelector<HTMLElement>(".lang-menu");
		if (!toggle || !menu) {
			return;
		}

		toggle.addEventListener("click", (event) => {
			event.stopPropagation();
			menu.classList.toggle("hidden");
		});
	});

	document.addEventListener("click", () => {
		dropdowns.forEach((dropdown) => {
			dropdown.querySelector<HTMLElement>(".lang-menu")?.classList.add("hidden");
		});
	});
}

function initAdminHeader(): void {
	const adminHeader = document.getElementById("admin-header");
	if (!adminHeader) {
		return;
	}

	initAdminHeaderHeight(adminHeader);
	initLanguageDropdowns();
}

document.addEventListener("DOMContentLoaded", initAdminHeader);
