namespace DungeonFlux.Model
{
    public enum RoomType
    {
        Empty,      // Пустая комната
        Treasure,   // Комната с сокровищами
        Enemy,      // Комната с врагами
        Boss,       // Комната с боссом
        Shop,       // Магазин
        Start,      // Начальная комната
        Exit,       // Выход / переход на следующий уровень
        Corridor,   // Коридор
        DeadEnd     // Тупик
    }
} 