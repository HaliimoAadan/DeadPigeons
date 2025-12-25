// Handles System.Text.Json ReferenceHandler.Preserve payloads.
// Supports: [ ... ] OR { $values: [...] } OR { $id, $values, ... } and resolves {$ref:"x"}.

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

export function toNumberArray(raw: unknown): number[] {
    const arr: any[] =
        Array.isArray(raw)
            ? raw
            : raw && typeof raw === "object" && Array.isArray((raw as any).$values)
                ? (raw as any).$values
                : [];

    return arr
        .map((x) => Number(x))
        .filter((n) => Number.isFinite(n))
        .sort((a, b) => a - b);
}