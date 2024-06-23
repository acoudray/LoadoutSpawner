import { DependencyContainer } from "tsyringe";
import type { IPreAkiLoadMod } from "@spt-aki/models/external/IPreAkiLoadMod";
import type { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import type { StaticRouterModService } from "@spt-aki/services/mod/staticRouter/StaticRouterModService";
import { InventoryHelper } from "@spt-aki/helpers/InventoryHelper";
import { ProfileHelper } from "@spt-aki/helpers/ProfileHelper";
import { IItemEventRouterResponse } from "@spt-aki/models/eft/itemEvent/IItemEventRouterResponse";
import { TProfileChanges } from "@spt-aki/models/eft/itemEvent/IItemEventRouterBase";
import { Item } from "@spt-aki/models/eft/common/tables/IItem";
import { HashUtil } from "@spt-aki/utils/HashUtil";
import { SaveServer } from "@spt-aki/servers/SaveServer";
import { ItemHelper } from "@spt-aki/helpers/ItemHelper";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { ArmorHelper } from "./helpers/ArmorHelper";

class Mod implements IPreAkiLoadMod {
    container: DependencyContainer;
    inventoryHelper: InventoryHelper = null;
    itemHelper: ItemHelper = null;
    profileHelper: ProfileHelper = null;
    saveServer: SaveServer = null;
    db: DatabaseServer = null;
    logger: ILogger = null;
    hashUtil: HashUtil = null;
    armorHelper: ArmorHelper;
    public preAkiLoad(container: DependencyContainer): void {
        this.logger = container.resolve<ILogger>("WinstonLogger");
        const staticRouterModService = container.resolve<StaticRouterModService>("StaticRouterModService");
        this.container = container;
        this.profileHelper = this.container.resolve<ProfileHelper>("ProfileHelper");
        this.inventoryHelper = this.container.resolve<InventoryHelper>("InventoryHelper");
        this.hashUtil = container.resolve<HashUtil>("HashUtil");
        this.saveServer = container.resolve<SaveServer>("SaveServer");
        this.itemHelper = container.resolve<ItemHelper>("ItemHelper");
        this.db = container.resolve<DatabaseServer>("DatabaseServer");
        this.armorHelper = new ArmorHelper(container);
        // Hook up a new static route
        staticRouterModService.registerStaticRouter(
            "LoadoutSpawnWeapon",
            [
                {
                    url: "/loadout-spawner/weapon",
                    action: (url, info, sessionId) => {
                        const output: IItemEventRouterResponse = {
                            warnings: [],
                            profileChanges: <TProfileChanges>{
                                [sessionId]: {
                                    items: {
                                        new: [],
                                        change: [],
                                        del: [],
                                    },
                                },
                            },
                        };
                        this.giveItem(sessionId, info.items, output);
                        return JSON.stringify(output.profileChanges[sessionId]);
                    },
                },
            ],
            "LoadoutSpawnWeapon"
        );

        staticRouterModService.registerStaticRouter(
            "LoadoutSpawnEquipment",
            [
                {
                    url: "/loadout-spawner/equipment",
                    action: (url, info, sessionId) => {
                        const output: IItemEventRouterResponse = {
                            warnings: [],
                            profileChanges: <TProfileChanges>{
                                [sessionId]: {
                                    items: {
                                        new: [],
                                        change: [],
                                        del: [],
                                    },
                                },
                            },
                        };
                        info.items.map((item: Item[]) => {
                            this.giveItem(sessionId, item, output);
                        });
                        return JSON.stringify(output.profileChanges[sessionId]);
                    },
                },
            ],
            "LoadoutSpawnEquipment"
        );
    }

    private giveItem(sessionId: string, item: Item[], output: IItemEventRouterResponse = null) {
        const pmcData = this.profileHelper.getPmcProfile(sessionId);
        const itemTpl = this.db.getTables().templates.items[item[0]._tpl];
        try {
            this.armorHelper.addChildrenToArmorModSlots(item, itemTpl);
        } catch {}
        this.inventoryHelper.addItemToStash(
            sessionId,
            {
                itemWithModsToAdd: this.itemHelper.replaceIDs(item, pmcData),
                foundInRaid: true,
                useSortingTable: false,
                callback: null,
            },
            pmcData,
            output
        );
        this.saveServer.saveProfile(sessionId);
    }
}
module.exports = { mod: new Mod() };
