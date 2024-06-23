"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ArmorHelper = void 0;
class ArmorHelper {
    itemHelper;
    hashUtil;
    constructor(container) {
        this.itemHelper = container.resolve("ItemHelper");
        this.hashUtil = container.resolve("HashUtil");
    }
    addChildrenToArmorModSlots(armor, itemDbDetails) {
        // Armor has no mods, make no additions
        const hasMods = itemDbDetails._props.Slots.length > 0;
        if (!hasMods) {
            return;
        }
        // Check for and add required soft inserts to armors
        const requiredSlots = itemDbDetails._props.Slots.filter((slot) => slot._required);
        const hasRequiredSlots = requiredSlots.length > 0;
        if (hasRequiredSlots) {
            for (const requiredSlot of requiredSlots) {
                const modItemDbDetails = this.itemHelper.getItem(requiredSlot._props.filters[0].Plate)[1];
                const plateTpl = requiredSlot._props.filters[0].Plate; // `Plate` property appears to be the 'default' item for slot
                if (plateTpl === "") {
                    // Some bsg plate properties are empty, skip mod
                    continue;
                }
                const mod = {
                    _id: this.hashUtil.generate(),
                    _tpl: plateTpl,
                    parentId: armor[0]._id,
                    slotId: requiredSlot._name,
                    upd: {
                        Repairable: {
                            Durability: modItemDbDetails._props.MaxDurability,
                            MaxDurability: modItemDbDetails._props.MaxDurability,
                        },
                    },
                };
                armor.push(mod);
            }
        }
        // Check for and add plate items
        const plateSlots = itemDbDetails._props.Slots.filter((slot) => this.itemHelper.isRemovablePlateSlot(slot._name));
        if (plateSlots.length > 0) {
            for (const plateSlot of plateSlots) {
                const plateTpl = plateSlot._props.filters[0].Plate;
                if (!plateTpl) {
                    // Bsg data lacks a default plate, skip adding mod
                    continue;
                }
                const modItemDbDetails = this.itemHelper.getItem(plateTpl)[1];
                armor.push({
                    _id: this.hashUtil.generate(),
                    _tpl: plateSlot._props.filters[0].Plate, // `Plate` property appears to be the 'default' item for slot
                    parentId: armor[0]._id,
                    slotId: plateSlot._name,
                    upd: {
                        Repairable: {
                            Durability: modItemDbDetails._props.MaxDurability,
                            MaxDurability: modItemDbDetails._props.MaxDurability,
                        },
                    },
                });
            }
        }
    }
}
exports.ArmorHelper = ArmorHelper;
//# sourceMappingURL=ArmorHelper.js.map