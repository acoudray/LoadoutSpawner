"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const ArmorHelper_1 = require("./helpers/ArmorHelper");
class Mod {
    container = null;
    inventoryHelper = null;
    itemHelper = null;
    profileHelper = null;
    saveServer = null;
    db = null;
    logger = null;
    hashUtil = null;
    armorHelper = null;
    preAkiLoad(container) {
        this.logger = container.resolve("WinstonLogger");
        const staticRouterModService = container.resolve("StaticRouterModService");
        this.container = container;
        this.profileHelper =
            this.container.resolve("ProfileHelper");
        this.inventoryHelper =
            this.container.resolve("InventoryHelper");
        this.hashUtil = container.resolve("HashUtil");
        this.saveServer = container.resolve("SaveServer");
        this.itemHelper = container.resolve("ItemHelper");
        this.db = container.resolve("DatabaseServer");
        this.armorHelper = new ArmorHelper_1.ArmorHelper(container);
        // Hook up a new static route
        staticRouterModService.registerStaticRouter("LoadoutSpawnWeapon", [
            {
                url: "/loadout-spawner/weapon",
                action: (url, info, sessionId) => {
                    const output = {
                        warnings: [],
                        profileChanges: {
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
        ], "LoadoutSpawnWeapon");
        staticRouterModService.registerStaticRouter("LoadoutSpawnEquipment", [
            {
                url: "/loadout-spawner/equipment",
                action: (url, info, sessionId) => {
                    const output = {
                        warnings: [],
                        profileChanges: {
                            [sessionId]: {
                                items: {
                                    new: [],
                                    change: [],
                                    del: [],
                                },
                            },
                        },
                    };
                    info.items.map((item) => {
                        this.giveItem(sessionId, item, output);
                    });
                    return JSON.stringify(output.profileChanges[sessionId]);
                },
            },
        ], "LoadoutSpawnEquipment");
    }
    giveItem(sessionId, item, output = null) {
        const pmcData = this.profileHelper.getPmcProfile(sessionId);
        const itemTpl = this.db.getTables().templates.items[item[0]._tpl];
        this.logger.info(JSON.stringify(itemTpl, null, 2));
        this.armorHelper.addChildrenToArmorModSlots(item, itemTpl);
        this.inventoryHelper.addItemToStash(sessionId, {
            itemWithModsToAdd: this.itemHelper.replaceIDs(item, pmcData),
            foundInRaid: true,
            useSortingTable: false,
            callback: null,
        }, pmcData, output);
        this.saveServer.saveProfile(sessionId);
    }
}
module.exports = { mod: new Mod() };
//# sourceMappingURL=mod.js.map