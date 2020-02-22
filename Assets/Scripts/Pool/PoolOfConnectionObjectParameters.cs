using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Структура, содержащая в себе основные параметры для индексирования связей прямоугольников в пуле
/// </summary>
public struct PoolOfConnectionObjectParameters : PoolObjectParameters
{
    public RectangleController rect1;
    public RectangleController rect2;

    /// <summary>
    /// Конструктор структуры
    /// </summary>
    /// <param name="_rect1">Первый прямоугольник</param>
    /// <param name="_rect2">Второй прямоугольник</param>
    public PoolOfConnectionObjectParameters(RectangleController _rect1, RectangleController _rect2)
    {
        rect1 = _rect1;
        rect2 = _rect2;
    }

    public static bool operator ==(PoolOfConnectionObjectParameters obj1, PoolOfConnectionObjectParameters obj2)
    {
        if ((obj1.rect1 == obj2.rect1 && obj1.rect2 == obj2.rect2) ||
            (obj1.rect1 == obj2.rect2 && obj1.rect2 == obj2.rect1)) return true;
        else
            return false;
    }

    public static bool operator !=(PoolOfConnectionObjectParameters obj1, PoolOfConnectionObjectParameters obj2)
    {
        if ((obj1.rect1 == obj2.rect1 && obj1.rect2 == obj2.rect2) ||
            (obj1.rect1 == obj2.rect2 && obj1.rect2 == obj2.rect1)) return false;
        else
            return true;
    }
}
