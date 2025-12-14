import {PlayersClient} from "@core/generated-client.ts";
import {baseUrl} from "@core/baseUrl.ts";
import {customFetch} from "@utilities/customFetch.ts";

export const playersApi = new PlayersClient(baseUrl, customFetch);