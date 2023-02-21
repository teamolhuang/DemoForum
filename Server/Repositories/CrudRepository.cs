namespace DemoForum.Repositories;

public interface ICrudRepository<T, in TKey>
{
    /// <summary>
    ///     Saves an entity into DB.
    /// </summary>
    /// <param name="obj">Entity</param>
    /// <returns>Saved entity</returns>
    T Create(T obj);

    /// <summary>
    ///     Read an entity by key.
    /// </summary>
    /// <param name="key">key</param>
    /// <returns>entity</returns>
    T Read(TKey key);

    /// <summary>
    ///     Updates an entity by key and input object.
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="obj">Entity</param>
    /// <returns>Updated entity</returns>
    T Update(TKey key, T obj);

    /// <summary>
    ///     Deletes an entity by key.
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Deleted entity</returns>
    T Delete(TKey key);
}