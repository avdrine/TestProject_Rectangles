using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Класс, описывающий контроллер игрового уровня
/// </summary>
public class SceneController : MonoBehaviour, IPointerClickHandler
{
    //Singleton
    public static SceneController Instance;

    #region Внешние объекты для инспектора
    //Объект пула связей
    [SerializeField][Tooltip("Объект пула связей")] private PoolOfConnection _poolOfConnections;
    //Объект пула прямоугольников
    [SerializeField][Tooltip("Объект пула прямоугольников")] private PoolOfRectangles _poolOfRectangles;
    //Основная камера на сцене
    [Tooltip("Основная камера на сцене")] public Camera mainCamera;
    #endregion

    //Первый выделенный прямоугольник для создания связи
    private RectangleController _rect1ToCreateConnection;

    private void Awake()
    {
        //Реализация синглтона - заполнение Instance
        if (Instance == null) Instance = this;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //При клике на экран пул прямоугольника инициализует новый прямоугольник
        _poolOfRectangles.InstantiateObject(mainCamera.ScreenToWorldPoint(eventData.position));
    }

    /// <summary>
    /// Функция, сбрасывающая выделение выделенного прямоугольника, если он соответсвует переданному
    /// </summary>
    /// <param name="_rect">Прямоугольник для проверки</param>
    public void TryToClearCheckedRect(RectangleController _rect)
    {
        if (_rect1ToCreateConnection != null && _rect == _rect1ToCreateConnection) _rect1ToCreateConnection = null;
    }

    /// <summary>
    /// Функция, создающая соединение между прямоугольниками (если это возможно).
    /// </summary>
    /// <param name="_rect">Прямоугольник для которого нужно создать связь</param>
    /// <returns>Успешность выполнения</returns>
    public bool TryToCreateConnection(RectangleController _rect)
    {
        //Если передан пустой параметр передать неуспешный результат
        if (_rect == null) return false;

        //Если первый выделенный прямоугольник не пустой, но при этом он ссылается 
        //на выключенный объект - очистить ссылку на первый выделенный прямоугольник
        if (_rect1ToCreateConnection != null && !_rect1ToCreateConnection.GetState()) _rect1ToCreateConnection = null;

        //Если пытаемся создать связь с одним и тем же прямоугольником
        if (_rect1ToCreateConnection == _rect)
        {
            //Снимаем выделение
            _rect.UncheckRect();
            //Очищаем ссылку на первый выделенный прямоугольник
            _rect1ToCreateConnection = null;

            //Возвращаем неудачный результат
            return false;
        }

        //Если ссылка на первый выделенный прямоугольник пуста
        if (_rect1ToCreateConnection == null)
        {
            //Сохраняем ссылку на первый выделенный прямоугольник
            _rect1ToCreateConnection = _rect;

            //Выделяем прямоугольник
            _rect1ToCreateConnection.CheckRect();
        }

        //Если ссылка на первый выделенный прямоугольник не пуста
        else
        {
            //Создаем параметры для создания связи прямоугольников
            PoolObjectParameters tmpParameters = (PoolObjectParameters) new PoolOfConnectionObjectParameters(_rect1ToCreateConnection, _rect);

            //Если в пуле связей не существует связи со идентичными параметрами
            if (!_poolOfConnections.IsPoolObjectExist(tmpParameters))
            {
                //Создадим объект связи
                _poolOfConnections.InstantiateObject(tmpParameters);

                //Снимаем выделение с прямоугольников, используемого для создания связи
                _rect1ToCreateConnection.UncheckRect();
                _rect.UncheckRect();

                //Очищаем ссылку на первый выделенный прямоугольник
                _rect1ToCreateConnection = null;
            }
                
        }

        //Возвратить успешный результат
        return true;
    }
}
