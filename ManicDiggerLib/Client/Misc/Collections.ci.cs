public class DictionaryStringAudioSample
{
    public DictionaryStringAudioSample()
    {
        max = 1024;
        count = 0;
        keys = new string[max];
        values = new AudioSampleCi[max];
    }

    string[] keys;
    AudioSampleCi[] values;
    int max;
    int count;

    public void Set(string key, AudioSampleCi value)
    {
        int index = GetIndex(key);
        if (index != -1)
        {
            values[index] = value;
            return;
        }
        keys[count] = key;
        values[count] = value;
        count++;
    }

    public bool Contains(string key)
    {
        int index = GetIndex(key);
        return index != -1;
    }

    public AudioSampleCi Get(string key)
    {
        int index = GetIndex(key);
        return values[index];
    }

    public int GetIndex(string key)
    {
        for (int i = 0; i < count; i++)
        {
            if (keys[i] == key)
            {
                return i;
            }
        }
        return -1;
    }
}

public class DictionaryStringAudioData
{
    public DictionaryStringAudioData()
    {
        keys = new string[max];
        for (int i = 0; i < max; i++)
        {
            keys[i] = null;
        }
        values = new AudioData[max];
        for (int i = 0; i < max; i++)
        {
            values[i] = null;
        }
    }
    internal string[] keys;
    internal AudioData[] values;
    internal const int max = 1024;

    internal AudioData GetById(int id)
    {
        return values[id];
    }

    internal bool Contains(string key)
    {
        int id = GetId(key);
        return id != -1;
    }

    internal int GetId(string key)
    {
        for (int i = 0; i < max; i++)
        {
            if (keys[i] == key)
            {
                return i;
            }
        }
        return -1;
    }

    internal int Set(string key, AudioData bmp)
    {
        int id = GetId(key);
        if (id != -1)
        {
            values[id] = bmp;
            return id;
        }
        for (int i = 0; i < max; i++)
        {
            if (keys[i] == null)
            {
                keys[i] = key;
                values[i] = bmp;
                return i;
            }
        }
        return -1;
    }
}


class DictionaryStringByteArray
{
    public DictionaryStringByteArray()
    {
        items = new StringByteArray[1024];
        itemsCount = 1024;
    }
    internal StringByteArray[] items;
    internal int itemsCount;

    internal void Set(string name, byte[] value)
    {
        for (int i = 0; i < itemsCount; i++)
        {
            if (items[i] == null) { continue; }
            if (Game.StringEquals(items[i].name, name))
            {
                items[i].data = value;
                return;
            }
        }
        for (int i = 0; i < itemsCount; i++)
        {
            if (items[i] == null)
            {
                items[i] = new StringByteArray();
                items[i].name = name;
                items[i].data = value;
                return;
            }
        }
    }

    internal byte[] Get(string name)
    {
        for (int i = 0; i < itemsCount; i++)
        {
            if (items[i] == null) { continue; }
            if (Game.StringEquals(items[i].name, name))
            {
                return items[i].data;
            }
        }
        return null;
    }
}

public class DictionaryVector3Float
{
    public DictionaryVector3Float()
    {
        itemsCount = 16 * 1024;
        items = new Vector3Float[itemsCount];
    }
    internal Vector3Float[] items;
    internal int itemsCount;
    internal bool ContainsKey(int x, int y, int z)
    {
        return ItemIndex(x, y, z) != -1;
    }

    int ItemIndex(int x, int y, int z)
    {
        for (int i = 0; i < itemsCount; i++)
        {
            if (items[i] == null)
            {
                continue;
            }
            Vector3Float item = items[i];
            if (item.x == x && item.y == y && item.z == z)
            {
                return i;
            }
        }
        return -1;
    }

    internal float Get(int x, int y, int z)
    {
        return items[ItemIndex(x, y, z)].value;
    }

    internal void Remove(int x, int y, int z)
    {
        if (ItemIndex(x, y, z) == -1)
        {
            return;
        }
        items[ItemIndex(x, y, z)] = null;
    }

    internal void Set(int x, int y, int z, float value)
    {
        int index = ItemIndex(x, y, z);
        if (index != -1)
        {
            items[index].value = value;
        }
        else
        {
            for (int i = 0; i < itemsCount; i++)
            {
                if (items[i] == null)
                {
                    Vector3Float item = new Vector3Float();
                    item.x = x;
                    item.y = y;
                    item.z = z;
                    item.value = value;
                    items[i] = item;
                    return;
                }
            }
        }
    }

    internal int Count()
    {
        int count = 0;
        for (int i = 0; i < itemsCount; i++)
        {
            if (items[i] != null)
            {
                count++;
            }
        }
        return count;
    }

    internal void Clear()
    {
        for (int i = 0; i < itemsCount; i++)
        {
            items[i] = null;
        }
    }
}

public class DictionaryStringString
{
    public DictionaryStringString()
    {
        Start(64);
    }

    public void Start(int count_)
    {
        items = new KeyValueStringString[count_];
        count = count_;
    }

    internal KeyValueStringString[] items;
    internal int count;

    public void Set(string key, string value)
    {
        for (int i = 0; i < count; i++)
        {
            if (items[i] == null)
            {
                continue;
            }
            if (Game.StringEquals(items[i].key, key))
            {
                items[i].value = value;
                return;
            }
        }
        for (int i = 0; i < count; i++)
        {
            if (items[i] == null)
            {
                items[i] = new KeyValueStringString();
                items[i].key = key;
                items[i].value = value;
                return;
            }
        }
    }

    internal bool ContainsKey(string key)
    {
        for (int i = 0; i < count; i++)
        {
            if (items[i] == null)
            {
                continue;
            }
            if (Game.StringEquals(items[i].key, key))
            {
                return true;
            }
        }
        return false;
    }

    internal string Get(string key)
    {
        for (int i = 0; i < count; i++)
        {
            if (items[i] == null)
            {
                continue;
            }
            if (Game.StringEquals(items[i].key, key))
            {
                return items[i].value;
            }
        }
        return null;
    }

    internal void Remove(string key)
    {
        for (int i = 0; i < count; i++)
        {
            if (items[i] == null)
            {
                continue;
            }
            if (Game.StringEquals(items[i].key, key))
            {
                items[i] = null;
            }
        }
    }
}

public class KeyValueStringString
{
    internal string key;
    internal string value;
}

public class DictionaryStringInt1024
{
    public DictionaryStringInt1024()
    {
        items = new KeyValueStringInt[max];
        count = 0;
    }
    internal KeyValueStringInt[] items;
    internal int count;
    const int max = 1024;

    /// <summary>
    /// Set the specified key to the specified value.
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Value to set</param>
    public void Set(string key, int value)
    {
        for (int i = 0; i < count; i++)
        {
            if (items[i] == null)
            {
                continue;
            }
            if (items[i].key == key)
            {
                items[i].value = value;
                return;
            }
        }
        for (int i = 0; i < count; i++)
        {
            if (items[i] == null)
            {
                items[i] = new KeyValueStringInt();
                items[i].key = key;
                items[i].value = value;
                return;
            }
        }
        KeyValueStringInt k = new KeyValueStringInt();
        k.key = key;
        k.value = value;
        items[count++] = k;
    }

    /// <summary>
    /// Check if the dictionary contains the specified key.
    /// This method is case-sensitive.
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns><b>true</b> if key is found</returns>
    internal bool Contains(string key)
    {
        for (int i = 0; i < count; i++)
        {
            if (items[i] == null)
            {
                continue;
            }
            if (Game.StringEquals(items[i].key, key))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Get the specified key.
    /// This method is case-sensitive.
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns><b>Stored value</b> when key is found in collection, <b>-1</b> otherwise.</returns>
    internal int Get(string key)
    {
        for (int i = 0; i < count; i++)
        {
            if (items[i] == null)
            {
                continue;
            }
            if (Game.StringEquals(items[i].key, key))
            {
                return items[i].value;
            }
        }
        return -1;
    }

    /// <summary>
    /// Remove the specified key.
    /// This method is case-sensitive.
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns><b>true</b> if key is found in collection, <b>false</b> otherwise.</returns>
    public bool Remove(string key)
    {
        for (int i = 0; i < count; i++)
        {
            if (items[i] == null)
            {
                continue;
            }
            if (Game.StringEquals(items[i].key, key))
            {
                items[i] = null;
                return true;
            }
        }
        return false;
    }
}

public class KeyValueStringInt
{
    internal string key;
    internal int value;
}

public class ListAction
{
    public static ListAction Create(int max_)
    {
        ListAction l = new ListAction();
        l.Start(max_);
        return l;
    }

    public void Start(int max_)
    {
        max = max_;
        items = new Action_[max_];
        count = 0;
    }

    internal int max;
    internal Action_[] items;
    internal int count;

    internal void Clear()
    {
        for (int i = 0; i < count; i++)
        {
            items[i] = null;
        }
        count = 0;
    }

    internal void RemoveAt(int index)
    {
        for (int i = index; i < count - 1; i++)
        {
            items[i] = items[i + 1];
        }
        count--;
    }

    internal int Count()
    {
        return count;
    }

    internal void Add(Action_ action)
    {
        items[count++] = action;
    }
}

public class ListConnectedPlayer
{
    public ListConnectedPlayer()
    {
        items = new ConnectedPlayer[1024];
        count = 0;
    }
    internal ConnectedPlayer[] items;
    internal int count;

    internal void Add(ConnectedPlayer connectedPlayer)
    {
        items[count++] = connectedPlayer;
    }

    internal void RemoveAt(int at)
    {
        for (int i = at; i < count - 1; i++)
        {
            items[i] = items[i + 1];
        }
        count--;
    }
}

public class QueueAction
{
    public QueueAction()
    {
        Start(128);
    }
    public static QueueAction Create(int max_)
    {
        QueueAction queue = new QueueAction();
        queue.Start(max_);
        return queue;
    }

    void Start(int max_)
    {
        max = max_;
        items = new Action_[max_];
        count = 0;
    }

    internal Action_[] items;
    internal int start;
    internal int count;
    internal int max;

    public void Enqueue(Action_ value)
    {
        if (count == max)
        {
            Resize(max * 2);
        }
        int pos = start + count;
        pos = pos % max;
        count++;
        items[pos] = value;
    }

    void Resize(int newSize)
    {
        Action_[] items2 = new Action_[newSize];
        for (int i = 0; i < max; i++)
        {
            items2[i] = items[(start + i) % max];
        }
        items = items2;
        start = 0;
        max = newSize;
    }

    public Action_ Dequeue()
    {
        Action_ ret = items[start];
        items[start] = null;
        start++;
        start = start % max;
        count--;
        return ret;
    }

    public int Count()
    {
        return count;
    }
}


public class QueueNetIncomingMessage
{
    public QueueNetIncomingMessage()
    {
        items = new NetIncomingMessage[1];
        itemsSize = 1;
        count = 0;
    }
    NetIncomingMessage[] items;
    int count;
    int itemsSize;

    internal int Count()
    {
        return count;
    }

    internal NetIncomingMessage Dequeue()
    {
        NetIncomingMessage ret = items[0];
        for (int i = 0; i < count - 1; i++)
        {
            items[i] = items[i + 1];
        }
        count--;
        return ret;
    }

    internal void Enqueue(NetIncomingMessage p)
    {
        if (count == itemsSize)
        {
            NetIncomingMessage[] items2 = new NetIncomingMessage[itemsSize * 2];
            for (int i = 0; i < itemsSize; i++)
            {
                items2[i] = items[i];
            }
            itemsSize = itemsSize * 2;
            items = items2;
        }
        items[count++] = p;
    }
}

public class QueueByteArray
{
    public QueueByteArray()
    {
        items = new ByteArray[1];
        itemsSize = 1;
        count = 0;
    }
    ByteArray[] items;
    int count;
    int itemsSize;

    internal int Count()
    {
        return count;
    }

    internal ByteArray Dequeue()
    {
        ByteArray ret = items[0];
        for (int i = 0; i < count - 1; i++)
        {
            items[i] = items[i + 1];
        }
        count--;
        return ret;
    }

    internal void Enqueue(ByteArray p)
    {
        if (count == itemsSize)
        {
            ByteArray[] items2 = new ByteArray[itemsSize * 2];
            for (int i = 0; i < itemsSize; i++)
            {
                items2[i] = items[i];
            }
            itemsSize = itemsSize * 2;
            items = items2;
        }
        items[count++] = p;
    }
}

public class QueueByte
{
    public QueueByte()
    {
        max = 1024 * 1024 * 5;
        items = new byte[max];
    }
    byte[] items;
    internal int start;
    internal int count;
    internal int max;

    public int GetCount()
    {
        return count;
    }

    public void Enqueue(byte value)
    {
        int pos = start + count;
        pos = pos % max;
        count++;
        items[pos] = value;
    }

    public byte Dequeue()
    {
        byte ret = items[start];
        start++;
        start = start % max;
        count--;
        return ret;
    }


    public void DequeueRange(byte[] data, int length)
    {
        for (int i = 0; i < length; i++)
        {
            data[i] = Dequeue();
        }
    }

    internal void PeekRange(byte[] data, int length)
    {
        for (int i = 0; i < length; i++)
        {
            data[i] = items[(start + i) % max];
        }
    }
}

public class QueueINetOutgoingMessage
{
    public QueueINetOutgoingMessage()
    {
        items = new INetOutgoingMessage[1];
        itemsSize = 1;
        count = 0;
    }
    INetOutgoingMessage[] items;
    int count;
    int itemsSize;

    internal int Count()
    {
        return count;
    }

    internal INetOutgoingMessage Dequeue()
    {
        INetOutgoingMessage ret = items[0];
        for (int i = 0; i < count - 1; i++)
        {
            items[i] = items[i + 1];
        }
        count--;
        return ret;
    }

    internal void Enqueue(INetOutgoingMessage p)
    {
        if (count == itemsSize)
        {
            INetOutgoingMessage[] items2 = new INetOutgoingMessage[itemsSize * 2];
            for (int i = 0; i < itemsSize; i++)
            {
                items2[i] = items[i];
            }
            itemsSize = itemsSize * 2;
            items = items2;
        }
        items[count++] = p;
    }
}


public class FastQueueInt
{
    public void Initialize(int maxCount)
    {
        this.maxCount = maxCount;
        values = new int[maxCount];
        Count = 0;
        start = 0;
        end = 0;
    }
    int maxCount;
    int[] values;
    internal int Count;
    int start;
    int end;
    public void Push(int value)
    {
        values[end] = value;
        Count++;
        end++;
        if (end >= maxCount)
        {
            end = 0;
        }
    }
    public int Pop()
    {
        int value = values[start];
        Count--;
        start++;
        if (start >= maxCount)
        {
            start = 0;
        }
        return value;
    }
    public void Clear()
    {
        Count = 0;
    }
}



public class FastStackInt
{
    public void Initialize(int maxCount)
    {
        valuesLength = maxCount;
        values = new int[maxCount];
    }
    int[] values;
    int valuesLength;
    internal int count;
    public void Push(int value)
    {
        while (count >= valuesLength)
        {
            int[] values2 = new int[valuesLength * 2];
            for (int i = 0; i < valuesLength; i++)
            {
                values2[i] = values[i];
            }
            values = values2;
            valuesLength = valuesLength * 2;
        }
        values[count] = value;
        count++;
    }
    public int Pop()
    {
        count--;
        return values[count];
    }
    public void Clear()
    {
        count = 0;
    }

    internal int Count_()
    {
        return count;
    }
}
