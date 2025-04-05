import './App.css';
import Login from './Pages/Login';
import Home from './Pages/Home';
import { BrowserRouter, Route, Routes } from 'react-router';
import Signup from './Pages/Signup';

function App() {

    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Home />} />
                <Route path="/login" element={<Login />} />
                <Route path="/signup" element={<Signup />} />
            </Routes> 
        </BrowserRouter>
    );
    
}

export default App;