declare global {
    interface Window {
        __ChartData: Record<string, { type: string; data: unknown }>;
        __AdminI18n: { lang: string; strings: Record<string, string> };
    }
}

export {};
