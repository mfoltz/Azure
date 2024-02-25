using ProjectM;

namespace V.Core.Tools
{
    public class MUSB_DOTS
    {
        public static ModifyUnitStatBuff_DOTS Cooldown = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.CooldownModifier,
            Value = 0.0f,
            ModificationType = ModificationType.Set,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS SunCharge = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.SunChargeTime,
            Value = 50000f,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS Hazard = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.ImmuneToHazards,
            Value = 1f,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS SunResist = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.SunResistance,
            Value = 10000f,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS Speed = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.MovementSpeed,
            Value = 15f,
            ModificationType = ModificationType.Set,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS PResist = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.PhysicalResistance,
            Value = 10000f,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS FResist = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.FireResistance,
            Value = 10000f,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS HResist = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.HolyResistance,
            Value = 10000f,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS SResist = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.SilverResistance,
            Value = 10000f,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS GResist = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.GarlicResistance,
            Value = 10000f,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS SPResist = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.SpellResistance,
            Value = 10000f,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS PPower = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.PhysicalPower,
            Value = 10000f,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS RPower = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.ResourcePower,
            Value = 10000f,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS SPPower = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.SpellPower,
            Value = 10000f,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS PHRegen = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.PassiveHealthRegen,
            Value = 10000f,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS HRecovery = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.HealthRecovery,
            Value = 10000f,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS MaxHP = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.MaxHealth,
            Value = 10000f,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS MaxYield = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.ResourceYield,
            Value = 10f,
            ModificationType = ModificationType.Multiply,
            Id = ModificationId.NewId(0)
        };

        public static ModifyUnitStatBuff_DOTS DurabilityLoss = new ModifyUnitStatBuff_DOTS()
        {
            StatType = UnitStatType.ReducedResourceDurabilityLoss,
            Value = -10000f,
            ModificationType = ModificationType.Add,
            Id = ModificationId.NewId(0)
        };
    }

    public class MUSB_Functions
    {
        // need to convert from string in config to stat type, make map for the above
        public static ModifyUnitStatBuff_DOTS GetStatType(string statType)
        {
            switch (statType)
            {
                case "CooldownModifier":
                    return MUSB_DOTS.Cooldown;

                case "SunChargeTime":
                    return MUSB_DOTS.SunCharge;

                case "ImmuneToHazards":
                    return MUSB_DOTS.Hazard;

                case "SunResistance":
                    return MUSB_DOTS.SunResist;

                case "MovementSpeed":
                    return MUSB_DOTS.Speed;

                case "PhysicalResistance":
                    return MUSB_DOTS.PResist;

                case "FireResistance":
                    return MUSB_DOTS.FResist;

                case "HolyResistance":
                    return MUSB_DOTS.HResist;

                case "SilverResistance":
                    return MUSB_DOTS.SResist;

                case "GarlicResistance":
                    return MUSB_DOTS.GResist;

                case "SpellResistance":
                    return MUSB_DOTS.SPResist;

                case "PhysicalPower":
                    return MUSB_DOTS.PPower;

                case "ResourcePower":
                    return MUSB_DOTS.RPower;

                case "SpellPower":
                    return MUSB_DOTS.SPPower;

                case "PassiveHealthRegen":
                    return MUSB_DOTS.PHRegen;

                case "HealthRecovery":
                    return MUSB_DOTS.HRecovery;

                case "MaxHealth":
                    return MUSB_DOTS.MaxHP;

                case "ResourceYield":
                    return MUSB_DOTS.MaxYield;

                case "ReducedResourceDurabilityLoss":
                    return MUSB_DOTS.DurabilityLoss;

                default:
                    return MUSB_DOTS.SPPower;
            }
        }
    }
}