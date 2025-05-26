using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace DungeonFlux.Model
{
    [DataContract]
    public enum RoomType
    {
        [EnumMember]
        [Description("Коридор")]
        Corridor,

        [EnumMember]
        [Description("Тупик")]
        DeadEnd,

        [EnumMember]
        [Description("Начальная комната")]
        Start,

        [EnumMember]
        [Description("Выход")]
        Exit
    }

    [DataContract]
    public enum RoomSubType
    {
        [EnumMember]
        [Description("Пустая комната")]
        Empty,

        [EnumMember]
        [Description("Комната с врагами")]
        Enemy,

        [EnumMember]
        [Description("Комната с сокровищами")]
        Treasure,

        [EnumMember]
        [Description("Магазин")]
        Shop,

        [EnumMember]
        [Description("Комната с боссом")]
        Boss
    }

    public static class RoomTypeExtensions
    {
        public static bool IsSpecial(this RoomType type)
        {
            return type == RoomType.Start || type == RoomType.Exit;
        }

        public static string GetDescription(this RoomType type)
        {
            var field = type.GetType().GetField(type.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute?.Description ?? type.ToString();
        }

        public static string GetDescription(this RoomSubType type)
        {
            var field = type.GetType().GetField(type.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute?.Description ?? type.ToString();
        }
    }
} 