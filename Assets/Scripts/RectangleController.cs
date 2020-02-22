using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Класс, описывающий событие, которое передает контроллер прямоугольника
/// </summary>
public class RectEvent : UnityEvent<RectangleController> { };

/// <summary>
/// Класс, описывающий контроллер прямоугольника
/// </summary>
public class RectangleController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, IPoolObject
{
    //Время создание последнего прямоугольника. Используется для отключения только что 
    //созданного прямоугольника в месте с недостаточным пространством для создания 
    public static float timeOfCreateLastRectangle;

    #region Внешние объекты для инспектора
    //Image для отрисовки прямоугольника
    [SerializeField][Tooltip("Image для отрисовки прямоугольника")] private Image _image;
    //Sprite для отображения выделенности прямоугольника
    [SerializeField][Tooltip("Sprite для отображения выделенности прямоугольника")] private Sprite _checkedSprite;
    #endregion

    //Переменная для хранения положения прямоугольника до старта перетаскивания
    private Vector3 _oldPosition;

    //Флаг перетаскивания
    private bool _onDrag = false;

    //Количество коллизий с другими прямоугольниками в момент перетаскивания
    private int _collisionsCount = 0;

    //Время включения прямоугольника
    [HideInInspector] public float _bornTime;

    #region События
    //Событие начала перетаскивания
    [HideInInspector] public UnityEvent RectStartMoving = new UnityEvent();
    //Событие завершения перетаскивания
    [HideInInspector] public UnityEvent RectEndMoving = new UnityEvent();
    //Событие отключения объекта
    [HideInInspector] public RectEvent ObjectDisabledEvent = new RectEvent();

    #endregion

    #region Функции интерфейса объектов пула
    /// <summary>
    /// Функция для включения и инициализации объекта
    /// </summary>
    /// <param name="_parameters">Параметры, необходимые для инициализации (необязательно)</param>
    /// <returns>Успешность инициализации</returns>
    public bool Enable(PoolObjectParameters _parameters = null)
    {
        //Фиксируем время создания
        _bornTime = Time.time;
        timeOfCreateLastRectangle = _bornTime;

        //Устанавливаем случайный цвет прямоугольника
        _image.color = new Color32((byte)Random.Range(0, 256), (byte)Random.Range(0, 256), (byte)Random.Range(0, 256), 255);

        //включаем объект
        gameObject.SetActive(true);

        //Возвращаем успешный результат
        return true;
    }

    /// <summary>
    /// Функция для получения параметров объекта, которые используются для индексации элементов пула
    /// </summary>
    /// <returns>Параметры объекта, используемые для индексации</returns>
    public PoolObjectParameters GetPoolObjectParameters()
    {
        return null;
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
    public void DisableObj()
    {
        SceneController.Instance.TryToClearCheckedRect(this);
        UncheckRect();
        gameObject.SetActive(false);
        ObjectDisabledEvent.Invoke(this);
    }

    #endregion

    /// <summary>
    /// Функция, включающая выделение у прямоугольника
    /// </summary>
    public void CheckRect()
    {
        _image.sprite = _checkedSprite;
        _image.type = Image.Type.Sliced;
    }

    /// <summary>
    /// Функция, выключающая выделение у прямоугольника
    /// </summary>
    public void UncheckRect()
    {
        _image.sprite = null;
    }

    #region События Unity
    public void OnDrag(PointerEventData eventData)
    {
        //Если зажата ЛКМ
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            //Перемещаем прямоугольник по координатам мышки
            transform.position = new Vector3(SceneController.Instance.mainCamera.ScreenToWorldPoint(eventData.pointerCurrentRaycast.screenPosition).x,
            SceneController.Instance.mainCamera.ScreenToWorldPoint(eventData.pointerCurrentRaycast.screenPosition).y, 0);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Если зажата ЛКМ
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            //Изменить состояние флага перетаскивания
            _onDrag = true;

            //Сохраняем исходное положение прямоугольника
            _oldPosition = transform.position;

            //Вызываем событие начала перетаскивания
            RectStartMoving.Invoke();

            //Меняем положение прямоугольника в иерархии на самое последнее место, 
            //чтобы он отображался поверх всех других прямоугольников
            transform.SetAsLastSibling();
        }
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Если зажата ЛКМ
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            //Если есть столкновения с другими прямоугольниками
            if (_collisionsCount > 0)
            {
                //Возвращаем прямоугольник в исходное положение до начала перетаскивания
                transform.position = _oldPosition;
            }
            //Изменить состояние флага перетаскивания
            _onDrag = false;

            //Сбрасываем количество коллизий
            _collisionsCount = 0;

            //Вызываем событие окончания перетаскивания
            RectEndMoving.Invoke();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Если мы кликаем второй раз ЛКМ
        if(eventData.clickCount > 1 && eventData.button == PointerEventData.InputButton.Left)
        {
            //Выключаем объект
            DisableObj();
        }
        //Если клик ПКМ
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            //Пытаемся создать линию связи или изменяем состояние выделения прямоугольника
            SceneController.Instance.TryToCreateConnection(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Если включено перетаскивание
        if (_onDrag)
        {
            // Увеличиваем количество коллизий
            _collisionsCount++; 
        }
            
        //Если объект был только что создан в месте с недостаточным пространством
        else if (_bornTime == timeOfCreateLastRectangle && Time.time - _bornTime < 0.1f)
        {
            //Выключаем объект
            DisableObj(); 
        }
            
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //Если включено перетаскивание
        if (_onDrag)
        {
            //Уменьшаем количество коллизий
            _collisionsCount--;
        }
    }
    #endregion

}
