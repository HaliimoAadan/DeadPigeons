export function asArray<T>(value: any): T[] {
    if (Array.isArray(value)) return value as T[];
    if (value && Array.isArray((value as any).$values)) return (value as any).$values as T[];
    return [];
}

// Simplified array normalizer (array or {$values: []})
export function normalizeArray<T>(value: any): T[] {
    if (Array.isArray(value)) return value as T[];
    if (value && Array.isArray((value as any).$values)) return (value as any).$values as T[];
    return [];
}

export function asNumberArray(value: any): number[] {
    return asArray<any>(value)
        .map((entry) => (typeof entry === "string" ? Number(entry) : entry))
        .filter((n): n is number => Number.isFinite(n))
        .sort((a, b) => a - b);
}

// Handles ReferenceHandler.Preserve lists where list items can be {$ref:"x"}
export function resolvePreservedArray<T>(raw: unknown): T[] {
    const root: any = raw;

    const values: any[] =
        Array.isArray(root)
            ? root
            : root && typeof root === "object" && Array.isArray(root.$values)
                ? root.$values
                : [];

    const idMap = new Map<string, any>();

    const walk = (node: any) => {
        if (!node || typeof node !== "object") return;

        const id = node.$id;
        if (typeof id === "string") idMap.set(id, node);

        if (Array.isArray(node)) {
            node.forEach(walk);
            return;
        }

        for (const key of Object.keys(node)) walk(node[key]);
    };

    walk(root);

    return values
        .map((item) => {
            if (item && typeof item === "object" && typeof item.$ref === "string") {
                return idMap.get(item.$ref) ?? null;
            }
            return item;
        })
        .filter(Boolean) as T[];
}

export function normalizeNumbers(raw?: unknown): number[] {
    if (Array.isArray(raw)) return raw.map(Number).filter(Number.isFinite);
    if (raw && typeof raw === "object" && Array.isArray((raw as any).$values)) {
        return (raw as any).$values.map(Number).filter(Number.isFinite);
    }
    return [];
}