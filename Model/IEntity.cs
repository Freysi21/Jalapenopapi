namespace BaseRestAPI.Model
{
    //Base interface for all models used on api too implement
    //Each model will implement this interface so the generic repository
    //implementation can make queries for id fields on our models

    //Types that implement this interface AND DO NOT have a primary key field called Id
    //will decorate the Id field with [NotMapped, JsonIgnoreAttribute] to explicitly
    //Tell the entity framework this is the maintained by the database
    //and will encapsulate there own id field with the interface one like so

    /*
        [NotMapped, JsonIgnoreAttribute]
        public override string Id {
            get {
                return LandsNrSkotvopn;
            } set {
                LandsNrSkotvopn = value;
            }
        }

    */
    public abstract class IEntity<T>
    {
        public abstract T Id { get; set;}
    }
}