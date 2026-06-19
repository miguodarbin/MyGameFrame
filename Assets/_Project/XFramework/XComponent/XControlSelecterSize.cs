using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(XSnapScrollRect))]
public class XSelectedScale : MonoBehaviour
{
    /*
     * 首先要得到场上的全部item的中心点
     * 然后还要得到perfect的中心点
     * 然后遍历全部的item：
     *  - 判断这个item的中心点距离perfect的中心点的距离，
     *    -如果距离大于了影响范围，那就把缩放控制为one
     *    -如果距离小于了影响范围，那就要控制缩放了，缩放规则是离item的中心点越近，缩放越大，放到最大是1.2
     *      --我需要想一个映射关系，也就是说距离越近，缩放乘以的系数越大，缩放系数 = 最大缩放 - distance/1000
     *  以上说的所有计算都是在父空间完成的
     */
    
    
    
    
}