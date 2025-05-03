using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace DungeonFlux.Model
{
    [DataContract]
    public enum RoomType
    {
        [EnumMember]
        [Description("Пустая комната")]
        Empty,

        [EnumMember]
        [Description("Комната с сокровищами")]
        Treasure,

        [EnumMember]
        [Description("Комната с врагами")]
        Enemy,

        [EnumMember]
        [Description("Комната с боссом")]
        Boss,

        [EnumMember]
        [Description("Магазин")]
        Shop,

        [EnumMember]
        [Description("Начальная комната")]
        Start,

        [EnumMember]
        [Description("Выход")]
        Exit,

        [EnumMember]
        [Description("Коридор")]
        Corridor,

        [EnumMember]
        [Description("Тупик")]
        DeadEnd
    }

    public static class RoomTypeExtensions
    {
        public static bool IsSpecial(this RoomType type)
        {
            return type != RoomType.Empty && type != RoomType.Corridor && type != RoomType.DeadEnd;
        }

        public static bool IsPassable(this RoomType type)
        {
            return type != RoomType.Empty;
        }

        public static string GetDescription(this RoomType type)
        {
            var field = type.GetType().GetField(type.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute?.Description ?? type.ToString();
        }
    }
} 