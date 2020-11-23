using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool<T> {

    private List<PoolObject<T>> _poolList;
    public delegate T CallbackFactory();

    private int _count;
    private bool _isDinamic = false;
    private PoolObject<T>.PoolCallBack _init;
    private PoolObject<T>.PoolCallBack _finalize;
    private CallbackFactory _factoryMethod;

    public Pool(int initialStock, CallbackFactory factoryMethod, PoolObject<T>.PoolCallBack initialize, PoolObject<T>.PoolCallBack finalize, bool isDinamic)
    {
        _poolList = new List<PoolObject<T>>();

        //Save parameters.
        _factoryMethod = factoryMethod;
        _isDinamic = isDinamic;
        _count = initialStock;
        _init = initialize;
        _finalize = finalize;

        //Create the inital stock.
        for (int i = 0; i < _count; i++)
        {
            _poolList.Add(new PoolObject<T>(_factoryMethod(), _init, _finalize));
        }
    }

    public PoolObject<T> GetPoolObject()
    {
        for (int i = 0; i < _count; i++)
        {
            if (!_poolList[i].IsActive)
            {
                _poolList[i].IsActive = true;
                return _poolList[i];
            }
        }
        if (_isDinamic)
        {
            PoolObject<T> po = new PoolObject<T>(_factoryMethod(), _init, _finalize);
            po.IsActive = true;
            _poolList.Add(po);
            _count++;
            return po;
        }
        return null;
    }

    public T GetObjectFromPool()
    {
        for (int i = 0; i < _count; i++)
        {
            if (!_poolList[i].IsActive)
            {
                _poolList[i].IsActive = true;
                return _poolList[i].GetObj;
            }
        }
        if (_isDinamic)
        {
            PoolObject<T> po = new PoolObject<T>(_factoryMethod(), _init, _finalize);
            po.IsActive = true;
            _poolList.Add(po);
            _count++;
            return po.GetObj;
        }
        return default(T);
    }

    public void DisablePoolObject(T obj)
    {
        foreach (PoolObject<T> poolObj in _poolList)
        {
            if (poolObj.GetObj.Equals(obj))
            {
                poolObj.IsActive = false;
                return;
            }
        }
    }
}