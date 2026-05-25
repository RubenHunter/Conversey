declare global {
    interface Window {
        __ChartData: Record<string, {
            type: string;
            data: unknown;
            options?: unknown;
            periods?: Array<{ id: string; label: string; isActive: boolean }>;
            periodData?: Record<string, unknown>;
        }>;
        __AdminI18n?: { lang?: string; strings?: Record<string, string> };
    }
}

export {};
