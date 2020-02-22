using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Класс, описывающий событие, которое передает контроллер связи прямоугольников
/// </summary>
public class RectConnectionEvent : UnityEvent<RectConnectionController> { };

/// <summary>
/// Класс, описывающий контроллер связи прямоугольников
/// </summary>
public class RectConnectionController : MonoBehaviour, IPoolObject
{
    #region Внешние объекты для инспектора
    //LineRenderer, отрисовывающий связь
    [SerializeField][Tooltip("LineRenderer, отрисовывающий связь")] private LineRenderer _lineRenderer;
    #endregion

    //Флаг режима отслеживания движения связанных прямоугольников
    private bool _moving = false;

    //Параметры для индексирования объекта в пуле (содержит связанные прямоугольники)
	private PoolOfConnectionObjectParameters _rects;

    //массив Vector3, используемый в качестве буфера для передачи позиций прямоугольников в lineRenderer.
	private Vector3[] _rectsPositions;


    #region События
    //Событие включение объекта
    [HideInInspector] public RectConnectionEvent ObjectEnabledEvent = new RectConnectionEvent();
    //Событие отключения объекта
    [HideInInspector] public RectConnectionEvent ObjectDisabledEvent = new RectConnectionEvent();
    #endregion

    #region Функции интерфейса объектов пула

    /// <summary>
    /// Функция для включения и инициализации объекта
    /// </summary>
    /// <param name="_parameters">Параметры, необходимые для инициализации (необязательно)</param>
    /// <returns>Успешность инициализации</returns>
    public bool Enable(PoolObjectParameters _parameters)
	{
        //Сохраняем локально параметры индексирования элемента, конвертируя в нужный формат
		_rects = (PoolOfConnectionObjectParameters)_parameters;

        //Если произошла ошибочная ситуация - возвратить неудачный результат
		if (_rects.rect1 == null || _rects.rect2 == null || _rects.rect1 == _rects.rect2) return false;

        //Сбрасываем флаг режима отслеживания
		_moving = false;

        //Подписываемся на события изменения состояний связанных прямоугольников
		_rects.rect1.ObjectDisabledEvent.AddListener(DisableObj);
		_rects.rect2.ObjectDisabledEvent.AddListener(DisableObj);

		_rects.rect1.RectStartMoving.AddListener(EnableMoving);
		_rects.rect2.RectStartMoving.AddListener(EnableMoving);

		_rects.rect1.RectEndMoving.AddListener(DisableMoving);
		_rects.rect2.RectEndMoving.AddListener(DisableMoving);

        //Обновляем позиции для отрисовки линии связи
		UpdatePositions();

        //Включаем объект
		gameObject.SetActive(true);

		//Вызываем событие включения объекта
		ObjectEnabledEvent.Invoke(this);

        //Возвращаем успешный результат выполнения функции
		return true;
	}

    /// <summary>
    /// Функция для получения параметров объекта, которые используются для индексации элементов пула
    /// </summary>
    /// <returns>Параметры объекта, используемые для индексации</returns>
	public PoolObjectParameters GetPoolObjectParameters()
	{
		return (PoolObjectParameters)_rects;
	}

    /// <summary>
    /// Получить текущее состояние активности (включен / выключен)
    /// </summary>
    /// <returns>Текущее состояние активности</returns>
	public bool GetState()
	{
		return gameObject.activeSelf;
	}

    /// <summary>
    /// Функция, выключающая объект
    /// </summary>
    /// <param name="_rect">Объект, который вызвал выключение связи</param>
	public void DisableObj(RectangleController _rect)
	{
		DisableObj();
	}

    /// <summary>
    /// Функция, выключающая объект
    /// </summary>
	public void DisableObj()
	{
        //Выключаем объект
		gameObject.SetActive(false);

        //Отписываемся от событий изменений состояний связанных прямоугольников
		_rects.rect1.ObjectDisabledEvent.RemoveListener(DisableObj);
		_rects.rect2.ObjectDisabledEvent.RemoveListener(DisableObj);

		_rects.rect1.RectStartMoving.RemoveListener(EnableMoving);
		_rects.rect2.RectStartMoving.RemoveListener(EnableMoving);

		_rects.rect1.RectEndMoving.RemoveListener(DisableMoving);
		_rects.rect2.RectEndMoving.RemoveListener(DisableMoving);

        //Вызываем событие отключения объекта
		ObjectDisabledEvent.Invoke(this);
	}

    #endregion

    /// <summary>
    /// Функция, включающая режим отслеживания движения связанных прямоугольников
    /// </summary>
    private void EnableMoving()
    {
        _moving = true;
    }

    /// <summary>
    /// Функция, выключающая режим отслеживания движения связанных прямоугольников
    /// </summary>
    private void DisableMoving()
    {
        _moving = false;
        UpdatePositions();
    }

    /// <summary>
    /// Функция, обновляющая положения точек в связанном LineRenderer 
    /// в зависимости от положения связанных прямоугольников
    /// </summary>
    private void UpdatePositions()
	{
        //Если что-то не так с массивом позиций - переинициализируем его
		if (_rectsPositions == null || _rectsPositions.Length != 2)
		{
			_rectsPositions = new Vector3[2];
		}

        //записываем в массив позиции связанных прямоугольников
		_rectsPositions[0] = _rects.rect1.transform.position;
		_rectsPositions[1] = _rects.rect2.transform.position;

        //Передаем в lineRenerer массив позиций
		_lineRenderer.SetPositions(_rectsPositions);
	}

    void Update () 
	{
        //Если включен режим отслеживания движения - обновлять позиции точек LineRenderer
		if(_moving)
		{
			UpdatePositions();
		}
	}

}
