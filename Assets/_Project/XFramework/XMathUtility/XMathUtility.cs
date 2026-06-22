using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 此类只是用于回顾数学知识，和为了跟讲师进度，有很多方法我都认为封装的多此一举。所以大部分方法都不会在外部调用，多在外部自己写写。尤其是点乘、叉乘、向量，要常练习。
/// ！前后点乘，左右叉乘！
/// 点乘：
/// 单位向量中，点乘为1方向相同，点乘-1方向相反，点乘为0方向垂直，点乘大于0，方向同向，点乘小于0方向相反，点乘结果就是这两个向量夹角的cos值。
/// 非单位向量中，仅可以用大于0，方向同向。小于0方向，方向相反，等于0方向垂直，不能用等于一来判断方向了，因为非单位向量的两个向量点乘，几何意义是向量a去乘以向量b在向量a上的投影
/// 叉乘：
/// 单位向量中，a叉乘b的结果就是一个带正负的向量。公式就是：a叉乘 b = n 乘 |a| 乘 |b| 乘 Sin夹角。
/// |a| 乘 |b| 乘 Sin夹角就是a和b两边组成的平行四边形的面积，而n是unity左手定则规定的，
/// n是一个带方向的单位向量，并且垂直于a b 向量组成的平行四边形，并且根据左手定则，a卷向b，大拇指在上的话n就是垂直于这个平行四边形的正面，大拇指在下的话，n就是垂直于这个平行四边形的背面，
/// 具体unity怎么算的n，是unity靠代数公式算的，继续深挖下去意义不大
/// </summary>
public class XMathUtility
{
    //============================================================== 角度弧度转换 ==============================================================
    /// <summary>
    /// 角度转弧度，直接传角度进来，返回弧度，内部就是封装了 deg * Mathf.Deg2Rad
    /// </summary>
    public static float Deg2Rad(float deg)
    {
        return deg * Mathf.Deg2Rad;
    }

    /// <summary>
    /// 弧度转角度，直接传弧度进来，返回角度，内部就是封装了 rad * Mathf.Rad2Deg
    /// </summary>
    public static float Rad2Deg(float rad)
    {
        return rad * Mathf.Rad2Deg;
    }


    //============================================================== 某平面两点距离 ==============================================================
    /// <summary>
    /// 获得一个距离，这个距离是在XY平面上，两点的距离
    /// </summary>
    public static float GetTwoPointsDistanceInXY(Vector3 startPoint, Vector3 endPoint)
    {
        startPoint.z = 0;
        endPoint.z = 0;
        return Vector3.Distance(startPoint, endPoint);
    }

    /// <summary>
    /// 获得一个距离，这个距离是在XZ平面上，两点的距离
    /// </summary>
    public static float GetTwoPointsDistanceInXZ(Vector3 startPoint, Vector3 endPoint)
    {
        startPoint.y = 0;
        endPoint.y = 0;
        return Vector3.Distance(startPoint, endPoint);
    }

    /// <summary>
    /// 判断在XY平面上的两点的距离，是否小于某个距离
    /// </summary>
    public static bool IsTwoPointsDistanceInXYLessThan(Vector3 startPoint, Vector3 endPoint, float distance)
    {
        return GetTwoPointsDistanceInXY(startPoint, endPoint) <= distance;
    }

    /// <summary>
    /// 判断在XZ平面上的两点的距离，是否小于某个距离
    /// </summary>
    public static bool IsTwoPointsDistanceInXZLessThan(Vector3 startPoint, Vector3 endPoint, float distance)
    {
        return GetTwoPointsDistanceInXZ(startPoint, endPoint) < distance;
    }


    //============================================================== 某点的位置判断相关 ==============================================================
    /// <summary>
    /// 判断某点是否在屏幕外,不能在update里频繁调用，如果在update里频繁调用，可以去自己去缓存一个主摄像机
    /// </summary>
    public static bool IsPointOutScreen(Vector3 point)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("找不到主摄像机");
            return false;
        }

        var screenPoint = camera.WorldToScreenPoint(point);

        if (screenPoint.z < 0)
        {
            return true;
        }

        if (screenPoint.x >= 0 && screenPoint.x <= Screen.width && screenPoint.y >= 0 && screenPoint.y <= Screen.height)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 用于检测一个点，是否在某个向量前方的指定扇形区域内
    /// </summary>
    /// <param name="fanOrigin">指的是这个扇形的起点</param>
    /// <param name="fanForward">指的是这个扇形的前方这个向量，方向也可以</param>
    /// <param name="fanHalfAngle">指的是这个扇形开合角度的一半</param>
    /// <param name="fanRadius">指的是这个扇形的半径</param>
    /// <param name="checkPoint">指的是要在扇形范围内检测的点</param>
    /// <returns></returns>
    public static bool IsPointInXZFanArea(Vector3 fanOrigin, Vector3 fanForward, float fanHalfAngle, float fanRadius, Vector3 checkPoint)
    {
        fanOrigin.y = 0;
        checkPoint.y = 0;
        fanForward.y = 0;
        fanForward.Normalize();

        //1.先判断距离，如果距离不吻合，那角度再怎么吻合都没用
        if (!IsTwoPointsDistanceInXZLessThan(fanOrigin, checkPoint, fanRadius))
        {
            return false;
        }

        //2.根据公式 a·b =|a||b|cosθ，单位向量中也就是cosθ = a·b，然后扇形的一半角度的cos就是 cos fanHalfAngle，然后目标点向量点乘fanForward得到的cos一定要大于 cos fanHalfAngle才能算在范围内
        var checkDirection = (checkPoint - fanOrigin).normalized;
        var cosTargetToOrigin = Vector3.Dot(fanForward, checkDirection);
        var cosFanHalfAngle = Mathf.Cos(Deg2Rad(fanHalfAngle));
        if (cosTargetToOrigin < cosFanHalfAngle)
        {
            return false;
        }

        return true;
    }


    //============================================================== 射线检测相关 ==============================================================
    /// <summary>
    /// 外部可以自己写无分配的检测，性能更好
    /// 通过传入射线，最大检测距离，和检测层级，如果检测到有东西会把这个东西产生的RaycastHit用回调的形式传递出去
    /// 是否检测Trigger，会跟着Edit → Project Settings → Physics → Queries Hit Triggers设置走
    /// </summary>
    public static void RayCast(Ray ray, UnityAction<RaycastHit> callback, float maxDistance, int layerMask)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, maxDistance, layerMask))
        {
            callback?.Invoke(hitInfo);
        }
    }

    /// <summary>
    /// 外部可以自己写无分配的检测，性能更好
    /// 通过传入射线，最大检测距离，和检测层级，如果检测到有东西会把这个东西的GameObject用回调的形式传递出去
    /// 是否检测Trigger，会跟着Edit → Project Settings → Physics → Queries Hit Triggers设置走
    /// </summary>
    public static void RayCast(Ray ray, UnityAction<GameObject> callback, float maxDistance, int layerMask)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, maxDistance, layerMask))
        {
            callback?.Invoke(hitInfo.collider.gameObject);
        }
    }

    /// <summary>
    /// 外部可以自己写无分配的检测，性能更好
    /// 通过传入射线，最大检测距离，和检测层级，如果检测到有东西会把这个东西的T类型脚本对象用回调的形式传递出去
    /// 是否检测Trigger，会跟着Edit → Project Settings → Physics → Queries Hit Triggers设置走
    /// </summary>
    public static void RayCast<T>(Ray ray, UnityAction<T> callback, float maxDistance, int layerMask)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, maxDistance, layerMask))
        {
            callback?.Invoke(hitInfo.collider.gameObject.GetComponent<T>());
        }
    }

    /// <summary>
    /// 外部可以自己写无分配的检测，性能更好
    /// 通过传入射线，最大检测距离，和检测层级，把检测到的所有东西产生的RaycastHit用回调的形式传递出去
    /// 外部的回调，写的逻辑是对每个检测到的对象所执行的逻辑
    /// 是否检测Trigger，会跟着Edit → Project Settings → Physics → Queries Hit Triggers设置走
    /// </summary>
    public static void RayCastAll(Ray ray, UnityAction<RaycastHit> callback, float maxDistance, int layerMask)
    {
        var hitInfos = Physics.RaycastAll(ray, maxDistance, layerMask);
        foreach (var hitInfo in hitInfos)
        {
            callback?.Invoke(hitInfo);
        }
    }

    /// <summary>
    /// 外部可以自己写无分配的检测，性能更好
    /// 通过传入射线，最大检测距离，和检测层级，把检测到的所有东西产生的GameObject用回调的形式传递出去
    /// 外部的回调，写的逻辑是对每个检测到的对象所执行的逻辑
    /// 是否检测Trigger，会跟着Edit → Project Settings → Physics → Queries Hit Triggers设置走
    /// </summary>
    public static void RayCastAll(Ray ray, UnityAction<GameObject> callback, float maxDistance, int layerMask)
    {
        var hitInfos = Physics.RaycastAll(ray, maxDistance, layerMask);
        foreach (var hitInfo in hitInfos)
        {
            callback?.Invoke(hitInfo.collider.gameObject);
        }
    }

    /// <summary>
    /// 外部可以自己写无分配的检测，性能更好
    /// 通过传入射线，最大检测距离，和检测层级，把检测到的所有东西身上各自的T类型脚本对象用回调的形式传递出去
    /// 外部的回调，写的逻辑是对每个检测到的对象所执行的逻辑
    /// 是否检测Trigger，会跟着Edit → Project Settings → Physics → Queries Hit Triggers设置走
    /// </summary>
    public static void RayCastAll<T>(Ray ray, UnityAction<T> callback, float maxDistance, int layerMask)
    {
        var hitInfos = Physics.RaycastAll(ray, maxDistance, layerMask);
        foreach (var hitInfo in hitInfos)
        {
            callback?.Invoke(hitInfo.collider.gameObject.GetComponent<T>());
        }
    }


    //============================================================== 范围检测相关 ==============================================================
    /// <summary>
    /// 外部可以自己写无分配的检测，性能更好
    /// 外部传入一个想要检测的一个盒子范围和想要检测到的类型，这里会把检测到的类型通过回调传递给外部
    /// 是否检测Trigger，会跟着Edit → Project Settings → Physics → Queries Hit Triggers设置走
    /// </summary>
    /// <param name="boxCenter">想要检测的盒子范围那个盒子的中心</param>
    /// <param name="boxHalfSize">盒子一半的尺寸，如：(半宽，半高，半深)</param>
    /// <param name="boxRotate">盒子旋转的四元数</param>
    /// <param name="layerMask">检测的层级位掩码</param>
    /// <param name="callback">外部通过这个回调执行对检测到的东西的逻辑</param>
    public static void OverlapBox<T>(Vector3 boxCenter, Vector3 boxHalfSize, Quaternion boxRotate, int layerMask, UnityAction<T> callback) where T : Object
    {
        var colliders = Physics.OverlapBox(boxCenter, boxHalfSize, boxRotate, layerMask);
        var type = typeof(T);
        foreach (var collider in colliders)
        {
            if (type == typeof(GameObject))
            {
                callback?.Invoke(collider.gameObject as T);
            }
            else if (type == typeof(Collider))
            {
                callback?.Invoke(collider as T);
            }
            else
            {
                callback?.Invoke(collider.gameObject.GetComponent<T>());
            }
        }
    }

    /// <summary>
    /// 
    /// 外部传入一个想要检测的一个球形范围和想要检测到的类型，这里会把检测到的类型通过回调传递给外部
    /// 是否检测Trigger，会跟着Edit → Project Settings → Physics → Queries Hit Triggers设置走
    /// </summary>
    public static void OverlapSphere<T>(Vector3 boxCenter, float radius, int layerMask, UnityAction<T> callback) where T : Object
    {
        var colliders = Physics.OverlapSphere(boxCenter, radius, layerMask);
        if (colliders.Length <= 0)
        {
            return;
        }

        var type = typeof(T);
        foreach (var collider in colliders)
        {
            if (type == typeof(GameObject))
            {
                callback?.Invoke(collider.gameObject as T);
            }
            else if (type == typeof(Collider))
            {
                callback?.Invoke(collider as T);
            }
            else
            {
                callback?.Invoke(collider.gameObject.GetComponent<T>());
            }
        }
    }
}