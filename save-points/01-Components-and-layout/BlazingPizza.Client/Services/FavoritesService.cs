namespace BlazingPizza.Client.Services
{
    //Servicio para almacenar pizzas favoritas
    public class FavoritesService
    {
        public List<PizzaSpecial> favorites { get; set; } = new List<PizzaSpecial>();
    }
}
