using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс, описывающий реализацию пула для связей прямоугольников
/// </summary>
public class PoolOfConnection : Pool 
{
    /// <summary>
    /// Функция, возвращающая элемент пула
    /// </summary>
    /// <param name="_searchParameters">Параметры поиска</param>
    /// <param name="_resultObject">Возращаемый элемент</param>
    /// <returns>Успешность поиска</returns>
    public new bool TryGetPoolObject(PoolObjectParameters _searchParameters, ref IPoolObject _resultObject)
    {
        //Конвертируем полученные параметры в нужный формат
        PoolOfConnectionObjectParameters searchParameters = (PoolOfConnectionObjectParameters)_searchParameters;

        //Перебираем пул активных элементов
        for (int i = 0; i < _poolOfEnabled.Count; i++)
        {
            //Если мы нашли нужный элемент
            if ((PoolOfConnectionObjectParameters)_poolOfEnabled[i].GetPoolObjectParameters() == searchParameters)
            {
                //Заполнить возвращаемый элемент
                _resultObject = _poolOfEnabled[i];

                //Возвратить, что поиск окончился успешно
                return true;
            }
        }
        //Возвратить, что поиск окончился неуспешно
        return false;
    }

    /// <summary>
    /// Фунция проверяющая наличие элемента в пуле
    /// </summary>
    /// <param name="_searchParameters">Параметры поиска</param>
    /// <returns>Наличие или отсутствие в пуле</returns>
    public new bool IsPoolObjectExist(PoolObjectParameters _searchParameters)
    {
        //Конвертируем полученные параметры в нужный формат
        PoolOfConnectionObjectParameters searchParameters = (PoolOfConnectionObjectParameters)_searchParameters;

        //Перебираем пул активных элементов
        for (int i = 0; i < _poolOfEnabled.Count; i++)
        {
            
            if ((PoolOfConnectionObjectParameters)_poolOfEnabled[i].GetPoolObjectParameters() == searchParameters)
            {
                //Возвратить, что поиск окончился успешно
                return true;
            }
        }
        //Возвратить, что поиск окончился неуспешно
        return false;
    }

    /// <summary>
    /// Функция, описывающая процесс инициализации элемента пула
    /// </summary>
    /// <param name="_createParameter">Параметры создания</param>
    public new void InstantiateObject(PoolObjectParameters _createParameter)
    {

        //Если пул выключенных объектов пуст
        if (_poolOfDisabled.Count == 0)
        {
            //Создаем новый элемент
            GameObject newItem = Instantiate(_poolObjectPrefab, transform);
            RectConnectionController rectConnectionController = newItem.GetComponent<RectConnectionController>();

            //Добавляем созданный прямоугольник в пул активных элементов
            _poolOfEnabled.Add(rectConnectionController);

            //Подписываемся на событие отключение только что созданного объекта
            rectConnectionController.ObjectDisabledEvent.AddListener(OnDisabledObj);
        }
        //Если в пуле выключенных объектов есть элементы
        else
        {
            //Переносим выключенный объект из пула неактивных элементов в пул активных
            _poolOfEnabled.Add(_poolOfDisabled[_poolOfDisabled.Count - 1]);
            _poolOfDisabled.RemoveAt(_poolOfDisabled.Count - 1);
        }
        //Включить созданный элемент
        _poolOfEnabled[_poolOfEnabled.Count - 1].Enable(_createParameter);
    }

    /// <summary>
    /// Функция, описывающая процесс отключения объекта и перемещение его в резерв
    /// </summary>
    /// <param name="_objParameter">Параметры объекта, которого необходимо отключить</param>
    public new void DisableObj(PoolObjectParameters _objParameter)
    {
        //Ищем указанный объект в пуле активных элементов
        for (int i = 0; i < _poolOfEnabled.Count; i++)
        {
            //Если нашли искомый объект
            if (_poolOfEnabled[i].GetPoolObjectParameters() == _objParameter)
            {
                //Отключить объект
                _poolOfEnabled[i].DisableObj();
                return;
            }
        }
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
