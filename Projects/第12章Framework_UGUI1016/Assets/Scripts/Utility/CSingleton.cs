

// 	CSingleton.cs
//	Author: jxw
//	2015-10-16



//Sigleton
public class CSingleton<T>
	where T : new()
{
	private static T m_sInstance;
	public static T sInstance
	{
		get
		{
			if(m_sInstance == null)
			{
				m_sInstance = new T();
			}
			return m_sInstance;
		}
	}

	//destroy instance
	public void Destroy()
	{
		m_sInstance = default(T);
	}
}