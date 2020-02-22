using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс, описывающий реализацию пула для прямоугольников
/// </summary>
public class PoolOfRectangles : Pool 
{
    /// <summary>
    /// Функция, описывающая процесс инициализации прямоугольника
    /// </summary>
    /// <param name="position">Позиция, в которой создается прямоугольник</param>
    public void InstantiateObject(Vector3 position)
    {
        //Создаем переменную под контроллер прямоугольника
        RectangleController rectangleController = null;

        //Если пул выключенных объектов пуст
        if (_poolOfDisabled.Count == 0)
        {
            //Создаем новый прямоугольник
            GameObject newItem = Instantiate(_poolObjectPrefab, transform);
            rectangleController = newItem.GetComponent<RectangleController>();

            //Добавляем созданный прямоугольник в пул активных элементов
            _poolOfEnabled.Add(rectangleController);

            //Подписываемся на событие отключение только что созданного объекта
            rectangleController.ObjectDisabledEvent.AddListener(OnDisabledObj);
        }
        //Если в пуле выключенных объектов есть элементы
        else
        {
            //Переносим выключенный объект из пула неактивных элементов в пул активных
            _poolOfEnabled.Add(_poolOfDisabled[_poolOfDisabled.Count - 1]);
            _poolOfDisabled.RemoveAt(_poolOfDisabled.Count - 1);
            rectangleController = (RectangleController)_poolOfEnabled[_poolOfEnabled.Count - 1];
        }

        //Устанавливаем позицию прямоугольника в указанную точку
        rectangleController.transform.position = new Vector3(position.x, position.y, 0);

        //Включить прямоугольник
        _poolOfEnabled[_poolOfEnabled.Count - 1].Enable();
    }

    /// <summary>
    /// Функция, описывающая процесс отключения объекта и перемещение его в резерв
    /// </summary>
    /// <param name="_object">Объект, который необходимо отключить</param>
    public new void DisableObj(IPoolObject _object)
    {
        //Ищем указанный объект в пуле активных элементов
        for (int i = 0; i < _poolOfEnabled.Count; i++)
        {
            //Если нашли искомый объект
            if (_poolOfEnabled[i] == _object)
            {
                //Отключить объект
                _poolOfEnabled[i].DisableObj();
                return;
            }
        }
    }

    /// <summary>
    /// Функция, описывающая происходящее при переходе объекта в отключенный режим.
    /// Необходимо эту функцию подписать на событие отключения в объектах пула.
    /// </summary>
    /// <param name="_object">Объект, который был только что отключен</param>
    public new void OnDisabledObj(IPoolObject _object)
    {
        //Ищем указанный объект в пуле активных элементов
        for (int i = 0; i < _poolOfEnabled.Count; i++)
        {
            //Если нашли искомый объект
            if (_poolOfEnabled[i] == _object)
            {
                //Переносим объект из пула активных элементов в пул неактивных
                _poolOfDisabled.Add(_poolOfEnabled[i]);
                _poolOfEnabled.RemoveAt(i);
                return;
            }
        }
    }
}
